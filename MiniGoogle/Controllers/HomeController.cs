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


namespace MiniGoogle.Controllers
{
    public class HomeController : Controller
    {
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

        public JsonResult doPageIndexing(string pageName, int pageID)
        {
            try
            {
                
                //this method runs recursively. 
                ContentSearchResult result = SearchLibrary.CreateIndexForPage(pageName, pageID);

                //  //now that the first page is indexed and the links are inserted, retrieve each of the pages in the links.
                List<LinkedPageData> pageLinks = DBSearchResult.GetLinkDataForPageID(result.PageID);


                foreach (LinkedPageData item in pageLinks)
                {
                    string fullURL = string.Join("", item.PageDirectory, item.PageName);
                    if (!DBSearchResult.PageContentIndexed(fullURL, item.PageName))
                    {
                        doPageIndexing(fullURL, item.PageID);
                    }
                }

                return Json("Indexing completed", JsonRequestBehavior.AllowGet);
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