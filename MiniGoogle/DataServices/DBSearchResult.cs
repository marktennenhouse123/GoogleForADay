using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MiniGoogle.Models;
using System.IO;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml;

namespace MiniGoogle.DataServices
{
    public class DBSearchResult
    {
       public static CloudDBEntities DB = new CloudDBEntities();

        //helper for finding existing pages to update title or other fields
        //for an already saved link.
        public static LinkedPageData GetPageInfo(int pageID)
        {
            var pageInfo = (from pg in DB.IndexedPages
                            where pg.PageID == pageID
                            select new LinkedPageData
                            {
                                PageID = pg.PageID,
                                PageURL = pg.PageURL,
                                ParentID = pg.ParentID.Value,
                                PageDirectory = pg.ParentDirectory,
                                PageName = pg.PageName
                                
                            }).First();
            return pageInfo;

        }

        //get the totals by page and by keyword.
        public static SearchTotal GetIndexedPageTotals(int indexedSiteID)
        {
            SearchTotal st = new SearchTotal();
            try
            {
                var pgCount = (from px in DB.IndexedPages
                               where px.IndexedSiteID == indexedSiteID
                               group px by px.PageURL into gr1
                               select new { myKey = gr1.Key, mycount = gr1.Count() }).ToList();
                st.PagesIndexed = pgCount.Sum(g => g.mycount);

                var kwCount = (from p in DB.IndexedPages
                               join pkw in DB.PageKeyWords
                               on p.PageID equals pkw.PageID
                               where p.IndexedSiteID == indexedSiteID
                               group p by p.PageName into gp
                               select new { myKey = gp.Key, myKWCount = gp.Count() }).ToList();
                st.KeywordsIndexed = kwCount.Sum(c => c.myKWCount);
                return st;
            }
            catch (Exception ex)
            {
                MessageLogger.LogThis(ex);
                return null;

            }
            
        }

        /// <summary>
        /// Get the number of unique parent ID values for the current root record.
        /// </summary>
        /// <param name="someLevel"></param>
        /// <returns></returns>
        public static bool GoneFarEnough(int someLevel, int siteIndexID)
        {
            var r = from pid in DB.IndexedPages
                    where pid.IsIndexed == true
                    && pid.IndexedSiteID == siteIndexID
                    select new { pid.ParentID };
           return  r.Distinct().Count() >= someLevel;
        }

        ///group the pages by pageURL and sum up the keyword counts
        public static List<KeywordRanking> GetKeywordRanking(string keyWord)
            {
            try
            {
                var results = (from pg in DB.IndexedPages
                               join pgLinks in DB.PageKeyWords
                               on pg.PageID equals pgLinks.PageID

                               where pgLinks.Keyword.Contains(keyWord) || pgLinks.Keyword.StartsWith(keyWord)
                               || null == keyWord
                               group new { pg, pgLinks } by pg.PageURL into grup1
                               select new KeywordRanking
                               {
                                   PageURL = grup1.FirstOrDefault().pg.PageURL,
                                   Title = grup1.FirstOrDefault().pg.Title,
                                   Rank = grup1.Sum(g => g.pgLinks.KeywordCount.Value)
                               }).ToList();


                return results;
            }
            catch (DbEntityValidationException ex)
            {
                MessageLogger.LogThis(ex);
                return null;
            }
            catch (Exception ex)
            {
                MessageLogger.LogThis(ex);
                return null;

            }
            }


        //Get any pages which still need to be indexed.
        public static List<LinkedPageData>  GetLinkDataForSiteIndexID(int indexedSiteID)
        {
            var result = (from pg in DB.IndexedPages
                          where pg.IndexedSiteID == indexedSiteID
                          && pg.IsIndexed != true
                          select new LinkedPageData
                          {
                              PageID = pg.PageID,
                              PageURL = pg.PageURL,
                              ParentID = pg.ParentID.Value,
                              PageName = pg.PageName,
                              PageDirectory = pg.ParentDirectory,
                              IndexedSiteID = pg.IndexedSiteID
                          }).ToList();

            return result;
        }

        //clears the AppLogs table.
        public static void ClearEventLog()
        { 
            try
            {
                
                var msgs = DB.AppLogs.ToList();
                foreach (var item in msgs)
                {
                    DB.AppLogs.Remove(item);
                    DB.SaveChanges();
                }
            }
           
            catch (Exception ex)
            {
                MessageLogger.LogThis(ex);
            }
        }

        //this resets the system by clearing all tables.
        public static void ClearAllSearchResults()
        {
           
                //use stored proc.. This is too slow.
                var keywordList = DB.PageKeyWords.ToList();
               
                    DB.PageKeyWords.RemoveRange(keywordList);
                    DB.SaveChanges();
                

                var links = DB.IndexedPages.ToList();
               
                    DB.IndexedPages.RemoveRange(links);
                    DB.SaveChanges();

            var sites = DB.IndexedSites.ToList();
            DB.IndexedSites.RemoveRange(sites);
            DB.SaveChanges();
          
        }

        //is the current page already indexed or not?
        //This is used to decide whether to index a link that came from a parent page.
        public static bool IsPageContentIndexed(string pageURL, string pageName)
        {
            Uri siteURL = new Uri(pageURL);
            string domainName = siteURL.GetLeftPart(UriPartial.Authority);
            var result = (from p in DB.IndexedPages
                          where p.PageURL.ToLower() == pageURL.ToLower()

                        && p.IsIndexed == true
                          select p).ToList();
            return result.Any();


        }

        //has the page been saved alread?
        //A link might have been inserted already. This avoids duplicates.
        public static bool IsPageAlreadySaved(string pageURL, string pageName)
        {try
            {
                Uri siteURL = new Uri(pageURL);
                string domainName = siteURL.GetLeftPart(UriPartial.Authority);
                var result = (from p in DB.IndexedPages
                              where p.PageName.ToLower() == pageName.ToLower()
                             && p.PageURL.StartsWith(domainName)
                             || p.PageURL == pageURL
                              select p).ToList();
                return result.Any();
            }

            catch (Exception ex)
            {
                MessageLogger.LogThis(ex);
                return true;
            }
                          
        }


        private static string RemoveEndingSlash(string URL)
        {
            string tempURL = URL;
            if (URL.EndsWith("/") || URL.EndsWith(@"\"))
            {
                tempURL = URL.Remove(URL.Length - 1, 1);
            }
            return tempURL;
        }
        //helper for building the URL of a page when it comes without the HTTP.
        //Get name of page, sometimes without the previous folder in front of it.
        public static string GetFileWithFolder(string singleLink)
        {
            string tempLink = RemoveEndingSlash(singleLink);



            if (singleLink.Contains("http"))
            {
                return Path.GetFileName(tempLink);
            }
            else
            {
                return singleLink;

            }
        }

        //get the URL that can be used for retrieving the content by passing in a single name
        //like /SomePage.html will be converted to http://mysite.com/somePage.html
        public static string GetFullURLFromPartial(string pageName, string parentDirectory)
        {

            if (parentDirectory.EndsWith("/"))
            {
                return string.Join("", parentDirectory, pageName);
            }
            else
            {
                return string.Join("/", parentDirectory, pageName);
            }
        }

       
        //Saves the links which are pulled from the HTML of a page.
        public static void SaveTheLinks(ContentSearchResult searchResults, IndexedPage pg)
        {
            try
            {   if (searchResults.Links.Count > 10)
                {   //for speed, remove the links so we can see if the rest of the system works .
                    searchResults.Links.RemoveRange(10, searchResults.Links.Count - 10);
                }
                               
                foreach (string singleLink in searchResults.Links)
                {
                    IndexedPage cp = new IndexedPage();
                    if (singleLink.Length > 1) //it might be only a /
                    {
                        cp.DateCreated = DateTime.Now;
                        cp.ParentID = pg.PageID;
                        cp.PageName = GetFileWithFolder(singleLink);
                        cp.IndexedSiteID = pg.IndexedSiteID;
                        //get directory for the file, not only the filename.
                        cp.ParentDirectory = Services.SearchLibrary.GetDirectoryForFile(singleLink, pg.PageID);
                        cp.PageURL = GetFullURLFromPartial(cp.PageName, cp.ParentDirectory);
                        cp.Title = ""; // THIS COMES ONLY FROM THE CONTENT;
                        

                        // code to avoid duplicates.
                        
                        if (IsValidLink(cp.PageURL) && !DBSearchResult.IsPageAlreadySaved(cp.PageURL, cp.PageName))
                        {
                            DB.IndexedPages.Add(cp);
                            DB.SaveChanges();
                        }
                    }
                }
            }
            catch (DbEntityValidationException ex)
            {
                var s = new Exception();


               string data= Services.SerializeIt.SerializeThis(searchResults);
                
                MessageLogger.LogThis(ex, data);
            }
            catch (Exception ex)
            {
                string data = Services.SerializeIt.SerializeThis(searchResults);
                MessageLogger.LogThis(ex,data);
            }

        }
        //some links are on the same page or is only the domain page..skip these.
        public static bool IsValidLink(string pageURL)
        {   // if the url is too short 
            //or is the same as the domain this will throw an error
            //and it can be skipped.

            try
            {
                Uri siteURL = new Uri(pageURL);
                string domainName = siteURL.GetLeftPart(UriPartial.Authority);

                if (pageURL.StartsWith("#"))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageLogger.LogThis(ex);
                return false;

            }

            return true;
        }


        //get the whole appLog. Normally, this would have a filter by appname or id.
        public static List<AppLogVM> GetAppLog()
        {
            var results = (from ap in DB.AppLogs
                           select new AppLogVM {
                             AppName = ap.AppName,
                             DateCreated = ap.DateCreated,
                             DateCreatedText = ap.DateCreated.ToString(),
                             EntityErrors = ap.EntityErrors,
                             FullMessage = ap.FullMessage,
                             FunctionName = ap.FunctionName,
                            MessageText = ap.MessageText,
                            ObjectData = ap.ObjectData,
                            PageName = ap.PageName
                           } ).ToList();
            return results;

            
        }

        //save each word and the # of times it occurs, word by word found on a parent page.
        public static void SaveTheKeywords(ContentSearchResult searchResults, IndexedPage pg)
        {
            
            try
            {
                //save the keywords for this page.
                foreach (KeywordRanking kw in searchResults.KeyWordRankingList)
                {
                    PageKeyWord pkw = new PageKeyWord();
                    pkw.PageID = pg.PageID;
                    pkw.Keyword = kw.Keyword;
                    pkw.KeywordCount = kw.Rank;
                    DB.PageKeyWords.Add(pkw);
                    DB.SaveChanges();

                }
            }
            catch (Exception ex)
            {
                string data = Services.SerializeIt.SerializeThis(searchResults.KeyWordRankingList);
                MessageLogger.LogThis(ex,data);
            }
        }


        //Update the page to Indexed so it will not be searched again.
        public static void UpdateIsIndexedFlag(int pageID)
        {//TODO: add code to handle failed page updates.
            var result = (from p in DB.IndexedPages
                          where p.PageID == pageID
                          select p).First();

            result.IsIndexed = true;
            DB.SaveChanges();
        }

        //Get the data for a page so it can be indexed.
        //used for requesting the content based on the URL.
        public static IndexedPage GetPageByName(string pageURL, string pageName)
        {

            try
            {
                Uri siteURL = new Uri(pageURL);
                string domainName = siteURL.GetLeftPart(UriPartial.Authority);
                var result = (from p in DB.IndexedPages
                              where p.PageName.ToLower() == pageName.ToLower()
                             && p.PageURL.StartsWith(domainName)
                             || p.PageURL == pageURL
                              select p).ToList();
                if (result.Any())
                {
                   return result.First();
                }
                else
                { return null;
                }
            }
            catch (Exception ex)
            {
                MessageLogger.LogThis(ex);
                return null;

            }
            
        }

        //get a group index for all the pages in a "run"
        //The IndexedSiteID is a grouping of all the pages that are indexed for a site.
        public static int GetNewSiteIndex(string domain, string page)
        { // insert to the SiteIndexes table
            
                IndexedSite ix = new IndexedSite();
                ix.Domain = domain;
                ix.InitialPage = page;
                DB.IndexedSites.Add(ix);
                DB.SaveChanges();
                return ix.IndexedSiteID;

        }


        //Main Method that calls several Save methods for the content, links and keywords of a page.
        public static int SaveSearchResults(ContentSearchResult searchResults)
        {
            try
            {
                

                int pageIDAfterInsert = 0;
                //save the  page 
                IndexedPage pg = new IndexedPage();

                pg.DateCreated = DateTime.Now;
                pg.ParentID = searchResults.ParentID;
                pg.PageName = GetFileWithFolder(searchResults.PageURL);
                pg.PageURL = searchResults.PageURL;
                pg.ParentDirectory = searchResults.ParentDirectory;
                pg.IndexedSiteID = searchResults.IndexedSiteID;
                pg.Title = searchResults.Title.Length > 50 ? searchResults.Title.Substring(0,49) : searchResults.Title;
                if (!IsPageAlreadySaved(pg.PageURL, pg.PageName))
                {
                    DB.IndexedPages.Add(pg);
                    DB.SaveChanges();
                    pageIDAfterInsert = pg.PageID;
                }
                else
                {   //the page already exists so add a few missing fields.

                    
                    pg = GetPageByName(pg.PageURL, pg.PageName);
                    pg.DateCreated = DateTime.Now;

                    pg.Title = searchResults.Title;

                    pageIDAfterInsert = pg.PageID;
                    DB.SaveChanges();
                }



                SaveTheLinks(searchResults, pg); //save the links for this page.


                SaveTheKeywords(searchResults, pg); //save the keywords


                UpdateIsIndexedFlag(pg.PageID);   //update the IsIndexed flag so it is not run again.


                return pageIDAfterInsert;
            }
            catch (DbEntityValidationException ex)
            { MessageLogger.LogThis(ex);
                return 0;
            }
            catch (Exception ex)
            {
                MessageLogger.LogThis(ex);
                return 0;

            }
        }
    }
}