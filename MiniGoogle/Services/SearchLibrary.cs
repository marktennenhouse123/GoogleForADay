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
        public static ContentSearchResult CreateIndexForPage(string pageURL, int parentID)
        {
            //check if this page has been indexed BEFORE getting the content.

            ContentSearchResult searchResult = new ContentSearchResult();
            searchResult.ParentID = parentID;
            searchResult.PageName = Path.GetFileName(pageURL);
            if (! DBSearchResult.PageContentIndexed(pageURL, searchResult.PageName))
            {
                searchResult.SearchContent = GetPageContent(pageURL);
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
       

        private static string GetTextFromHTML(string docText)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(docText);
            var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
            var nodeText = bodyNode.InnerText;
            return nodeText;

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
                                
                singleRanking.Keyword = grp.Key; //the key is each word
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