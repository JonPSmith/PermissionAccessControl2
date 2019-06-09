// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAuthorize;

namespace DataLayer.AppClasses.MultiTenantParts
{

    /// <summary>
    /// This contains the class that all the hierarchical tenant classes inherit from
    /// </summary>
    public class TenantBase : IDataKey
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
        /// This holds the DataKey, which is hierarchical in nature, working down from 
        /// i.e. it has the PK of each parent as hex strings, with a | at the end of each hex string.
        /// e.g. 1|2|4
        /// </summary>
        public string DataKey { get; private set; }

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

        //----------------------------------------------------
        // methods

        public void LinkToParent(TenantBase parent)
        {
            Parent = parent ?? throw new ApplicationException($"The parent cannot be null.");
        }

        public void SetAccessKey()
        {
            if (!(this is Company) && Parent == null)
                throw new ApplicationException($"The parent cannot be null if this tenant isn't a {nameof(Company)}.");
            if (TenantItemId == 0)
                throw new ApplicationException("This class must have a primary key set before calling SetDataKey.");

            DataKey = $"{TenantItemId:x}|";
            if (Parent != null)
                DataKey = Parent.DataKey + DataKey;
        }


    }
}