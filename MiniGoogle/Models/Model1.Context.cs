﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CloudDBEntities : DbContext
    {
        public CloudDBEntities()
            : base("name=CloudDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AppLog> AppLogs { get; set; }
        public virtual DbSet<PageKeyWord> PageKeyWords { get; set; }
        public virtual DbSet<IndexedSite> IndexedSites { get; set; }
        public virtual DbSet<IndexedPage> IndexedPages { get; set; }
    }
}
