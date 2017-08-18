using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MiniGoogle.Services;
using MiniGoogle.Models;
using MiniGoogle.DataServices;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Net;


namespace MiniGoogle.Controllers
{
    public class HomeController : Controller
    {

        //configured in Web.config
        int NUMBER_OF_LEVELS = 0;
         bool LimitReached { get; set; }
       
        
        public ActionResult Index()
        {
            return View();
        }

        //clear application log of all messages.
        public JsonResult ClearAppLog()
        {
            DBSearchResult.ClearEventLog();
            return Json("App Log Cleared", JsonRequestBehavior.AllowGet);
        }


        //this wipes out all of the search results.
        public JsonResult ClearAll()
        {
            DataServices.DBSearchResult.ClearAllSearchResults();
            return Json("All records deleted.");
        }

        /// <summary>
        /// Search for the keyword in all pages.
        /// 
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns> returns  list Keyword Ranking by page and # of occurences with link to each.
        /// </returns>
        public JsonResult RunSearch(string keyword)
        {
            List<KeywordRanking> rankingList = DBSearchResult.GetKeywordRanking(keyword);
            return Json(rankingList, JsonRequestBehavior.AllowGet);

        }

        //retrieve application log for debugging.
        public JsonResult GetAppLog()
        {
            List<AppLogVM> LogResults = DBSearchResult.GetAppLog();
            return Json(LogResults, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Main Indexing process starts here.
        /// Step 1: Create an IndexedSiteID for grouping all the upcoming pages
        /// Step 2: Make Recursive call to DoPageIndexing. This will loop on itself.
        /// 
        /// </summary>
        /// <param name="pageName"></param>
        /// <returns></returns>
        public JsonResult startPageIndexProcess(string pageName) {
            try
            {
                NUMBER_OF_LEVELS = Int16.Parse(ConfigurationManager.AppSettings["HowManyLevels"]);
                string Folder = SearchLibrary.GetDirectoryForFile(pageName, -1);
                string actualPage = System.IO.Path.GetFileName(pageName);

                //create a record to serve as a groupID  for the site or group of pages to index.
                int siteIndexID = DBSearchResult.GetNewSiteIndex(Folder, actualPage);
                return doPageIndexing(pageName, -1, siteIndexID);
            }
            catch (Exception ex)
            {
                MessageLogger.LogThis(ex);
                //Run query to return results.

            }
            return null;
          
        }
        /// <summary>
        /// This is the main workhorse which runs recursively.
        /// It will stop once the GoneFarEnough returns a true value for the LimitReached.
        /// LimitReached is in the webconfig. It controls the # of levels to walk/traverse.
        /// The pageName is the current url to index
        /// The ParentID is the ID of the page which contains the link.
        /// The SiteIndexID is the ID assigned to the site or group of related pages which is being indexed
        /// </summary>
        /// <param name="pageName"></param>
        /// <param name="parentID"></param>
        /// <param name="siteIndexID"></param>
        /// <returns></returns>
        public JsonResult doPageIndexing(string pageName, int parentID, int siteIndexID)
        {
            SearchTotal finalCount;
            try
            {
                //this method runs recursively.
                
                ContentSearchResult csr  =SearchLibrary.LoadPageContent(pageName, parentID, siteIndexID);
                SearchLibrary.GetLinksAndKeywords(csr);
                csr.PageID = DBSearchResult.SaveSearchResults(csr);
                
                //  //now that the first page is indexed and the links are inserted, retrieve each of the pages in the links.
                List<LinkedPageData> pageLinks = DBSearchResult.GetLinkDataForSiteIndexID(csr.IndexedSiteID);
                foreach (LinkedPageData item in pageLinks)
               
                {
                    string fullURL = string.Join("", item.PageDirectory, item.PageName);
                    if (!DBSearchResult.IsPageContentIndexed(fullURL, item.PageName))
                    {
                        bool limitReached = DBSearchResult.GoneFarEnough(NUMBER_OF_LEVELS, item.IndexedSiteID.Value);
                        if (!limitReached)
                        {
                            return doPageIndexing(fullURL, item.ParentID, siteIndexID);
                        }
                      
                    }

                }
              }
            catch (DbEntityValidationException ex)
            {
                MessageLogger.LogThis(ex);
                Server.ClearError();
            }
            catch (Exception ex)
            {
                MessageLogger.LogThis(ex);
                Server.ClearError();


            }
            finally {
                 finalCount = DBSearchResult.GetIndexedPageTotals(siteIndexID);
                
            }

            return Json(finalCount, JsonRequestBehavior.AllowGet);
        }
        }
    }
    
