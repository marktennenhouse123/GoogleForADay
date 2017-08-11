using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MiniGoogle.Models;
using System.IO;
using System.Data.Entity.Validation;
using System.Diagnostics;

namespace MiniGoogle.DataServices
{
    public class DBSearchResult
    {
       public static CloudDBEntities DB = new CloudDBEntities();

        public static LinkedPageData GetPageInfo(int pageID)
        {
            var pageInfo = (from pg in DB.IndexedPages
                            where pg.PageID == pageID
                            select new LinkedPageData
                            {
                                PageID = pg.PageID,
                                PageURL = pg.PageURL,
                                ParentID = pg.ParentID,
                                PageDirectory = pg.ParentDirectory,
                                PageName = pg.PageName
                                
                            }).First();
            return pageInfo;

        }

        public static List<KeywordRanking> GetKeywordRanking(string keyWord)
            {
            var keywordList = (from pg in DB.IndexedPages
                              join pgLinks in DB.PageKeywords
                              on pg.PageID equals pgLinks.PageID
                               where pgLinks.Keyword.Contains(keyWord)
                               || pgLinks.Keyword.StartsWith(keyWord)
                              select new KeywordRanking
                              {
                                  PageName = pg.PageName,
                                  Keyword = pgLinks.Keyword,
                                  Rank = pgLinks.KeywordCount.Value
                              }).ToList();
            return keywordList;
                
            
            }



        public static List<LinkedPageData>  GetLinkDataForPageID(int pageID)
        {
            var result = (from pg in DB.IndexedPages
                          where pg.ParentID == pageID
                          && pg.IsIndexed != true
                          select new LinkedPageData
                          {
                              PageID = pg.PageID,
                              PageURL = pg.PageURL,
                              ParentID = pg.ParentID,
                              PageName = pg.PageName,
                              PageDirectory = pg.ParentDirectory
                          }).ToList();

            return result;
        }

        public static void ClearEventLog()
        {
            //use stored proc..this is too slow.
            var msgs = DB.AppLogs.ToList();
            foreach (var item in msgs) {
                DB.AppLogs.Remove(item);
                DB.SaveChanges();
            }
        }

        public static void ClearAllSearchResults()
        {
            //use stored proc.. This is too slow.
           var keywordList = DB.PageKeywords.ToList();
            foreach (var item in keywordList)
            {
                DB.PageKeywords.Remove(item);
                DB.SaveChanges();
            }

            var links = DB.IndexedPages.ToList();
            foreach (var item in links)
            {
                DB.IndexedPages.Remove(item);
                DB.SaveChanges();
            }
           

        }
        public static bool PageContentIndexed(string pageURL, string pageName)
        {
            Uri siteURL = new Uri(pageURL);
            string domainName = siteURL.GetLeftPart(UriPartial.Authority);
            var result = (from p in DB.IndexedPages
                          where p.PageName.ToLower() == pageName.ToLower()
                          && p.PageURL.StartsWith(domainName)
                          && p.IsIndexed == true
                          select p).ToList();
            return result.Any();


        }


        public static bool PageAlreadySaved(string pageURL, string pageName)
        {try
            {
                Uri siteURL = new Uri(pageURL);
                string domainName = siteURL.GetLeftPart(UriPartial.Authority);
                var result = (from p in DB.IndexedPages
                              where p.PageName.ToLower() == pageName.ToLower()
                             && p.PageURL.StartsWith(domainName)
                              select p).ToList();
                return result.Any();
            }

            catch (Exception ex)
            {
                MessageLogger.LogThis(ex);
                return true;
            }
                          
        }

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

        public static string GetObjectAsString(IndexedPage pg)
        {
            System.Diagnostics.Trace.WriteLine(pg);
            StringWriter sw = new StringWriter();
            System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(pg.GetType());
            StringReader sr = new StringReader(sw.ToString());
            x.Serialize(sw, pg);
            sw.Close();
            string objectData = sr.ReadToEnd();
            return objectData;
        }

        public static void SaveTheLinks(ContentSearchResult searchResults,
            IndexedPage currentLink, IndexedPage pg)
        {
            try
            {
                
                foreach (string singleLink in searchResults.Links)
                {
                    IndexedPage cp = new IndexedPage();
                    if (singleLink.Length > 1) //it might be only a /
                    {
                        cp.DateCreated = DateTime.Now;
                        cp.ParentID = pg.PageID;
                        cp.PageName = Path.GetFileName(singleLink);

                        //get directory for the file, not only the filename.
                        cp.ParentDirectory = Services.SearchLibrary.GetDirectoryForFile(singleLink, pg.PageID);
                        cp.PageURL = GetFullURLFromPartial(cp.PageName, cp.ParentDirectory);

                        //extra serializable copy of current record for logging/errors
                        currentLink = cp;

                        // code to avoid duplicates.
                        
                        if (!DBSearchResult.PageAlreadySaved(cp.PageURL, cp.PageName))
                        {
                            DB.IndexedPages.Add(cp);
                            DB.SaveChanges();
                        }
                    }
                }
            }
            catch (DbEntityValidationException ex)
            {
              //  string data = GetObjectAsString(currentLink);
                MessageLogger.LogThis(ex, "");
            }
            catch (Exception ex)
            {
             //   string data = GetObjectAsString(currentLink);
                MessageLogger.LogThis(ex);
            }

        }

        public static List<AppLogVM> GetAppLog()
        {
            var results = (from ap in DB.AppLogs
                           select new AppLogVM {
                             AppName = ap.AppName,
                             DateCreated = ap.DateCreated,
                             EntityErrors = ap.EntityErrors,
                             FullMessage = ap.FullMessage,
                             FunctionName = ap.FunctionName,
                            MessageText = ap.MessageText,
                            ObjectData = ap.ObjectData,
                            PageName = ap.PageName
                           } ).ToList();
            return results;

            
        }
        public static void SaveTheKeywords(ContentSearchResult searchResults, IndexedPage pg)
        {
            //save the keywords for this page.
            foreach (KeywordRanking kw in searchResults.KeyWordRankingList)
            {
                PageKeyword pkw = new PageKeyword();
                pkw.PageID = pg.PageID;
                pkw.Keyword = kw.Keyword;
                pkw.KeywordCount = kw.Rank;
                DB.PageKeywords.Add(pkw);
                DB.SaveChanges();

            }
        }

        public static void UpdateIsIndexedFlag(int pageID)
        {
            var result = (from p in DB.IndexedPages
                          where p.PageID == pageID
                          select p).First();

            result.IsIndexed = true;
            DB.SaveChanges();
        }

        public static IndexedPage GetPageIndexByName(string pagename, string pageURL)
        {
            var pg = (from p in DB.IndexedPages
                      where p.PageName == pagename && p.PageURL.StartsWith(pageURL)
                      select p).First();
            return pg;
        }


        public static int SaveSearchResults(ContentSearchResult searchResults)
        {
            //currentlink is only for serializing to log/errors
            IndexedPage currentLink = new IndexedPage();

            int pageIDAfterInsert = 0;
            //save the  page 
            IndexedPage pg = new IndexedPage();
            pg.DateCreated = DateTime.Now;
            pg.ParentID = searchResults.ParentID;
            pg.PageName = Path.GetFileName(searchResults.PageURL);
            pg.PageURL = searchResults.PageURL;
            pg.ParentDirectory = searchResults.ParentDirectory;
            if (!PageAlreadySaved(pg.PageURL, pg.PageName))
            {
                DB.IndexedPages.Add(pg);
                DB.SaveChanges();
                pageIDAfterInsert = pg.PageID;
            }
            else
            {   //the page already exists so get the page ID.
                Uri siteURL = new Uri(pg.PageURL);
                string domainName = siteURL.GetLeftPart(UriPartial.Authority);

                IndexedPage ip = GetPageIndexByName(pg.PageName, domainName);
                pg.PageID = ip.PageID;
                pageIDAfterInsert = pg.PageID;
            }

            
            
            SaveTheLinks(searchResults, currentLink, pg); //save the links for this page.


            SaveTheKeywords(searchResults, pg); //save the keywords


            UpdateIsIndexedFlag(pg.PageID);   //update the IsIndexed flag so it is not run again.


            return pageIDAfterInsert;
        }
    }
}