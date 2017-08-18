﻿using System.Collections.Generic;
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

using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Text;

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

        /// <summary>GetLinksAndKeywords
        ///  the content of the page is loaded. So extract the links and text from a single page.
        /// //then load all of them into the main container= ContentSearchResult
        ///Main Object-ContentSearchResult: Container object for all the properties.
        /////This method loads and then passes the container to the save method later. 
        //Extracts links, title, and converts html to text content
        /// then counts up the keywords from the content.
        /// </summary>
        /// <param name="ContentSearchResult"></param>
     
        public static void GetLinksAndKeywords(ContentSearchResult sr)
        {
            
            //check if this page has been indexed BEFORE getting the content.
            try
            {
               if (!DBSearchResult.IsPageContentIndexed(sr.PageURL , sr.PageName))
                {   sr.Title = GetPageTitle(sr.SearchContent, sr.PageName);
                    sr.ParentDirectory = GetDirectoryForFile(sr.PageURL, sr.ParentID);
                    sr.PageURL = sr.PageURL;
                    sr.TextContent = GetTextFromHTML(sr.SearchContent);

                    //use the full page content to extract the links
                    sr.Links = GetLinks(sr.SearchContent);

                    //use ONLY the cleaned text to find the keyword ranking.
                    sr.KeyWordRankingList = GetKeywordCounts(sr.TextContent);
               }
               
            }
            catch (DbEntityValidationException ex)
            {
                string data = Services.SerializeIt.SerializeThis(sr);
                MessageLogger.LogThis(ex, data);
                
            }
            catch (Exception ex)
            {
                MessageLogger.LogThis(ex);
                
            }
        }


        public static ContentSearchResult LoadPageContent(string pageURL, int parentID, int siteIndexID)
        {
            ContentSearchResult searchResult = null;
            searchResult = new ContentSearchResult();
            //check if this page has been indexed BEFORE getting the content.
            try
            {
                
                searchResult.ParentID = parentID;
                searchResult.PageName = GetFilenameFromURL(pageURL);
                searchResult.IndexedSiteID = siteIndexID;
                searchResult.PageURL = pageURL;
                
                if (!DBSearchResult.IsPageContentIndexed(pageURL, searchResult.PageName))
                {
                    searchResult.SearchContent = GetPageContent(pageURL);

                }
                return searchResult;
            }

            catch (AggregateException ex)
            {                
                MessageLogger.LogThis(ex);
                return searchResult;
            }
            catch (Exception ex)
            {
                MessageLogger.LogThis(ex);
                return searchResult;
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