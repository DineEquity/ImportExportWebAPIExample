﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ImportExportWebAPIExample
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;    
    using System.Reflection;

    public partial class APBMenuCatalogEntities : DbContext
    {
        public APBMenuCatalogEntities()
            : base("name=APBMenuCatalogEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<FamilyGroup> FamilyGroups { get; set; }
        public virtual DbSet<Hierarchy> Hierarchies { get; set; }
        public virtual DbSet<MajorGroup> MajorGroups { get; set; }
        public virtual DbSet<MenuItemClass> MenuItemClasses { get; set; }
        public virtual DbSet<MenuItemDefinition> MenuItemDefinitions { get; set; }
        public virtual DbSet<MenuItemMaster> MenuItemMasters { get; set; }
        public virtual DbSet<MenuItemPrice> MenuItemPrices { get; set; }
    }

}
