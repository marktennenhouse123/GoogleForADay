using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HtmlAgilityPack;

namespace MiniGoogle.Models
{
    public class ContentSearchResult
    {
        public List<string> Links { get; set; }
        public List<KeywordRanking> KeyWordRankingList { get; set; }
        public string PageName { get; set; }
        public string ParentDirectory { get; set; }
        public string SearchContent { get; set; }
        public string TextContent { get; set; }
        public int ParentID { get; set; }
        public string PageURL { get; set; }
        public int PageID { get; set; }
        public string Title { get; set; }
        
    }
}