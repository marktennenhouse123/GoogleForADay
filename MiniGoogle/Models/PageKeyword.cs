//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MiniGoogle.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class PageKeyWord
    {
        public int PageKeywordID { get; set; }
        public Nullable<int> PageID { get; set; }
        public string Keyword { get; set; }
        public Nullable<int> KeywordCount { get; set; }
    
        public virtual IndexedPage IndexedPage { get; set; }
    }
}
