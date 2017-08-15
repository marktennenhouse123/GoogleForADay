using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using System.IO;
using MiniGoogle.Models;
using System.Text.RegularExpressions;
using MiniGoogle.DataServices;
using System.Web;
using System.Net;
using System;
using System.Diagnostics;
using System.Data.Entity.Validation;


namespace MiniGoogle.Services
{
    
    public class SearchLibrary
    {
        //Get the parent folder of a page.
        //if it has no HTTP we need to find the parent from the parent 
        // of the first page which is in the DB
        public static string GetDirectoryForFile(string pageURL, int parentID)
            {
            string fixedURL = RemoveEndingSlash(pageURL);
            if (!fixedURL.Contains("http"))
            {
                //retrieve the path from the database.
                LinkedPageData pg = DBSearchResult.GetPageInfo(parentID);
                return pg.PageDirectory;
            }
            else
            {
                //the URL might contain only a # or only the domain name.
                //handle the case where it is a root folder.
                Regex domainMatch;
                string protocolPart = @"^(http(s)?(:\/\/))?(www\.)?";
                string domainPart = @"[a-zA-Z0-9-_\.]+";
                string paramsPart = @"/([-a-zA-Z0-9:%_\+.~#?&//=]*)/";
                string fullURLMatch = string.Join("", protocolPart, domainPart, paramsPart);
                domainMatch = new Regex(fullURLMatch);
               string leftOver = domainMatch.Replace(fixedURL, "");
                if (leftOver.Length < 2)
                {
                    return fixedURL;
                }
                else
                {

                    var myRequest = new Uri(fixedURL);
                    string lastPart = myRequest.Segments.Last() + myRequest.Query;

                    string parentFolder = fixedURL.Replace(lastPart, "");

                    return parentFolder;
                }
            }
        }

        /// <summary>
        /// Load a page and then extract the links and text from a single page.
        /// //then load all of them into the main container= ContentSearchResult
        ///Main Object-ContentSearchResult: Container object for all the properties.
        /////This method loads and then passes the container to the save method. 
        //this saves all the links, and keywords 
        /// </summary>
        /// <param name="pageURL"></param>
        /// <returns></returns>
        public static ContentSearchResult CreateIndexForPage(string pageURL, int parentID, int siteIndexID)
        {
            ContentSearchResult searchResult = null;
            //check if this page has been indexed BEFORE getting the content.
            try
            {
                 searchResult = new ContentSearchResult();
                searchResult.ParentID = parentID;
                searchResult.PageName = GetFilenameFromURL(pageURL);
                searchResult.IndexedSiteID = siteIndexID;
                if (!DBSearchResult.IsPageContentIndexed(pageURL, searchResult.PageName))
                {
                    searchResult.SearchContent = GetPageContent(pageURL);
                    searchResult.Title = GetPageTitle(searchResult.SearchContent, searchResult.PageName);
                    searchResult.ParentDirectory = GetDirectoryForFile(pageURL, parentID);
                    searchResult.PageURL = pageURL;
                    searchResult.TextContent = GetTextFromHTML(searchResult.SearchContent);

                    //use the full page content to extract the links
                    searchResult.Links = GetLinks(searchResult.SearchContent);

                    //use ONLY the cleaned text to find the keyword ranking.
                    searchResult.KeyWordRankingList = GetKeywordCounts(searchResult.TextContent);

                    // save the results to the database for the Links and the Keyword ranking.
                    int newPageID = DBSearchResult.SaveSearchResults(searchResult);
                    searchResult.PageID = newPageID;
                }
                return searchResult;
            }
            catch (DbEntityValidationException ex)
            {
                string data = Services.SerializeIt.SerializeThis(searchResult);
                MessageLogger.LogThis(ex, data);
                return null;
            }
            catch (Exception ex)
            {
                MessageLogger.LogThis(ex);
                return null;
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

        private static string GetFilenameFromURL(string URL)
        {
            string fixedURL = RemoveEndingSlash(URL);
           return Path.GetFileName(fixedURL);
        }

        
        //extract the title (if it has one) from the HTML
        private static string GetPageTitle(string content, string pageName)
        {

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(content);
            var titleNode = doc.DocumentNode.SelectSingleNode("//title");
            if (titleNode != null)
            {
                var titleText = titleNode.InnerText;
                return titleText;
            }
            else
            { return pageName;
            }

        }


        //Get the body from the HTML if it can be extracted.
        private static string GetTextFromHTML(string docText)
        { try
            {   //find a way to remove script contents and tags.
               

                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(docText);
                var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
                if (bodyNode != null)
                {

                    var nodeText = bodyNode.InnerText;
                    
                    return nodeText;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                MessageLogger.LogThis(ex);
                return string.Empty;
            }
        }
      

        /// <summary>
        /// Return the text of the document.
        /// </summary>
        /// <param name="pageURL"></param>
        /// <returns>This returns the Head and body of the document</returns>
        private static string GetPageContent(string pageURL)
        { try
            {     //this is a good place to remove the scripts from the content.
                Regex scriptRemoval1 = new Regex(@"(?s)<script.*?(/>|</script>)");
                Regex scriptRemoval2 = new Regex(@"(?s)<noscript.*?(/>|</noscript>)");
                HtmlWeb web = new HtmlWeb();
                HtmlDocument doc = web.Load(pageURL);
                string pageContent = doc.DocumentNode.InnerHtml;
                string NoScriptContent = scriptRemoval1.Replace(pageContent, "");
                 NoScriptContent = scriptRemoval2.Replace(NoScriptContent, "");
                return NoScriptContent;
            }
            catch (Exception ex)
            {
                MessageLogger.LogThis(ex);
                return "";
            }
        }


        // for each word find the # of times it occurs in the page.
        private static List<KeywordRanking> GetKeywordCounts(string textOfPage)
        { 
            
            List<KeywordRanking> rankingList = new List<KeywordRanking>();

            char[] splitter = "".ToCharArray(); //split on spaces.
            List<string> wordsInPage = textOfPage.Split(splitter).ToList();

            //group and order the words using Linq
            var groupedWords = wordsInPage.GroupBy(w => w)
                                            .Where(w => w.Key.Trim().Length > 0)
                                            .OrderByDescending(w => w.Count());
            
            //each unique words gets grouped, then save the group name and the count in an object.
            
            foreach (var grp in groupedWords)
            {
               //main object to save and retrieve the keywork ranking =count and name of each word.
                KeywordRanking singleRanking = new KeywordRanking();
                                
                singleRanking.Keyword =  grp.Key.Length > 50 ? grp.Key.Substring(0,50): grp.Key; //the key is each word
                singleRanking.Rank = grp.Count();
                rankingList.Add(singleRanking);
            }
            
            return rankingList;

        }


        //extract the links from the HTML.
        //uses Null Guard helper class in case the nodes return an error.
        public static List<string>   GetLinks(string docText)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(docText);
            var hrefList = doc.DocumentNode.SelectNodes("//a").NullGuard()
                              .Select(p => p.GetAttributeValue("href", "not_found"))
                              .ToList();
            return hrefList;
        }
    }
}