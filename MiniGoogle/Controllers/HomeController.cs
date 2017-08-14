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

namespace MiniGoogle.Controllers
{
    public class HomeController : Controller
    {


        int NUMBER_OF_LEVELS = 0;
         bool LimitReached { get; set; }
             
        List<LinkedPageData> PreviousPageLinks { get; set; }   //this is used for handling pages that cannot be indexed.

        public ActionResult Index()
        {
            return View();
        }

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
        /// </summary>
        /// <param name="keyword"></param>
        /// <returns></returns>
        public JsonResult RunSearch(string keyword)
        {
            List<KeywordRanking> rankingList = DBSearchResult.GetKeywordRanking(keyword);
            return Json(rankingList, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetAppLog()
        {
            List<AppLogVM> LogResults = DBSearchResult.GetAppLog();
            return Json(LogResults, JsonRequestBehavior.AllowGet);
        }

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
            }
            return null;
          
        }
        /// <summary>
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
            try
            {
                //this method runs recursively. 
                ContentSearchResult result = SearchLibrary.CreateIndexForPage(pageName, parentID, siteIndexID);
                if (result.PageID <= 0)
                { string message = string.Format("CreateIndexForPage Failed to create index for page{0} parentID {1}, siteindex {2}, line 83", pageName, parentID, siteIndexID);
                    Exception ex = new Exception(message);

                    MessageLogger.LogThis(ex);
                    
                }
                
                //  //now that the first page is indexed and the links are inserted, retrieve each of the pages in the links.
                List<LinkedPageData> pageLinks = DBSearchResult.GetLinkDataForSiteIndexID(result.IndexedSiteID);
                //if (PreviousPageLinks == null)
                //{
                //    PreviousPageLinks = pageLinks;
                //}
              
                                
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
                        else
                        {
                            SearchTotal finalCount = DBSearchResult.GetIndexedPageTotals(siteIndexID);
                            return Json( finalCount, JsonRequestBehavior.AllowGet);
                        }
                        
                    }

                }

                //now go get the totals.
                return new JsonResult(); // get query totals.
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
        }
    }
    
