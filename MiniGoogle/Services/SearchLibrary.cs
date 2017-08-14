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
        public static string GetDirectoryForFile(string pageURL, int parentID)
            {
            if (! pageURL.Contains("http"))
            {
                //retrieve the path from the database.
                LinkedPageData pg = DBSearchResult.GetPageInfo(parentID);
                return pg.PageDirectory;
            }
            else
            { 
                //the URL might contain only a # or only the domain name.

            var myRequest = new Uri(pageURL);
                string lastPart = myRequest.Segments.Last() + myRequest.Query;
            string parentFolder = pageURL.Replace(lastPart, "");
          
                return parentFolder;
            }
        }
      
        /// <summary>
        /// Load a page and then extract the links and text from a single page.
        /// 
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
                searchResult.PageName = Path.GetFileName(pageURL);
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

        public static void FixEndlessLoop(List<LinkedPageData> PreviousPageLinks, List<LinkedPageData> currentPageLinks)
        {
            if (PreviousPageLinks.Count != currentPageLinks.Count)
            {
                if (PreviousPageLinks == currentPageLinks)
                { //this means we are stuck in an endless loop unable to handle a certain URL.

                    var resultOfComparing = currentPageLinks.Except(PreviousPageLinks).ToList();
                    if (resultOfComparing.Count == 0)
                    {
                        //update these as already indexed or failed.
                        foreach (var item in currentPageLinks)
                        {

                            DBSearchResult.UpdateIsIndexedFlag(item.PageID);

                        }

                    }
                    else
                    {
                        PreviousPageLinks = currentPageLinks;
                    }

                }

            }
            else if (PreviousPageLinks.Equals(null))
            {
                //this is only when the count == 0
                PreviousPageLinks = currentPageLinks;
            }
        }

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

        private static string GetTextFromHTML(string docText)
        { try
            {
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
        /// <returns></returns>
        private static string GetPageContent(string pageURL)
        {
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(pageURL);
            return doc.DocumentNode.InnerHtml;
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
               
                KeywordRanking singleRanking = new KeywordRanking();
                                
                singleRanking.Keyword =  grp.Key.Length > 50 ? grp.Key.Substring(0,50): grp.Key; //the key is each word
                singleRanking.Rank = grp.Count();
                rankingList.Add(singleRanking);
            }
            
            return rankingList;

        }



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