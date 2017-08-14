using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MiniGoogle.Models
{
    public class LinkedPageData
    {
        public string PageURL {get;set;}
        public string PageDirectory { get; set; }
        public int PageID { get; set; }
        public int ParentID { get; set; }
        public string PageName { get; set; }
        public int? IndexedSiteID { get; set; }
    }
}