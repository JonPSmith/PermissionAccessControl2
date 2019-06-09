// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAuthorize;
using DataLayer.EfCode;
using GenericServices;

namespace DataLayer.AppClasses.MultiTenantParts
{

    /// <summary>
    /// This contains the class that all the hierarchical tenant classes inherit from
    /// </summary>
    public class TenantBase : DataKeyBase
    {
        private TenantBase(){} // needed by EF Core

        protected TenantBase(string name, TenantBase parent)
        {
            Name = name;
            Parent = parent;
        }

        /// <summary>
        /// Each tenant has its own primary key (as well as the AccessKey used for multi-tenant filtering)
        /// </summary>
        [Key]
        public int TenantItemId { get; private set; }

        /// <summary>
        /// This is the name of the tenant: could be CompanyName, Area/Group or retail outlet name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// This is the foreign key that points to its parent - null if if its the Company tenant class
        /// </summary>
        public int? ParentItemId { get; private set; }

        /// <summary>
        /// This is a pointer to its parent - it will be null if its the Company tenant class
        /// </summary>
        [ForeignKey(nameof(ParentItemId))]
        public TenantBase Parent { get; private set; }

        /// <summary>
        /// This holds the tenants one level below 
        /// </summary>
        public ICollection<TenantBase> Children { get; private set; }

        //---------------------------------------------------
        //access methods

        public IStatusGeneric AddChild(TenantBase newChild, AppDbContext context)
        {
            if (Children == null)
            {
                
            }
        }

    }
}