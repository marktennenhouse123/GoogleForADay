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
    
    public partial class IndexedSite
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public IndexedSite()
        {
            this.IndexedPages = new HashSet<IndexedPage>();
        }
    
        public int IndexedSiteID { get; set; }
        public string Domain { get; set; }
        public string InitialPage { get; set; }
        public Nullable<System.DateTime> DateCreated { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<IndexedPage> IndexedPages { get; set; }
    }
}
