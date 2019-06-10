// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using DataAuthorize;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

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
        /// This holds the DataKey, which is hierarchical in nature, contains a string that reflects the
        /// position of the tenant in the hierarchy. I do this by building a string which contains the PK
        /// i.e. it has the PK of each parent as hex strings, with a | as a separator and a * at the end.
        /// e.g. 1|2|F*
        /// </summary>
        [MaxLength(DataAuthConstants.HierarchicalKeySize)]
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

        public override string ToString()
        {
            return $"{GetType().Name}: Name = {Name}, DataKey = {DataKey ?? "<null>"}";
        }

        //----------------------------------------------------
        // methods

        public void LinkToParent(TenantBase parent)
        {
            Parent = parent ?? throw new ApplicationException($"The parent cannot be null.");
        }

        /// <summary>
        /// This adds a child to the Children collection, BUT it does NOT handle 
        /// </summary>
        /// <param name="child"></param>
        /// <param name="context"></param>
        public void AddChild(TenantBase child, AppDbContext context)
        {
            if (this is RetailOutlet)
                throw new ApplicationException($"You can't add a child to a {nameof(RetailOutlet)}.");

            if (Children == null)
            {
                if (context.Entry(this).State == EntityState.Detached)
                    Children = new List<TenantBase>();
                else
                {
                    context.Entry(this).Collection(x => x.Children).Load();
                }
            }
            Children.Add(child);

        }

        /// <summary>
        /// This sets the DataKey to create the hierarchical DataAccess key.
        /// See <see cref="DataKey"/> for more on the format of the hierarchical DataAccess key.
        /// </summary>
        public void SetDataKeyFromHierarchy()
        {
            if (!(this is Company) && Parent == null)
                throw new ApplicationException($"The parent cannot be null if this tenant isn't a {nameof(Company)}.");
            if (TenantItemId == 0)
                throw new ApplicationException("This class must have a primary key set before calling SetHierarchicalDataKey.");

            var ending = this is RetailOutlet ? "*" : "|";
            DataKey = $"{TenantItemId:x}{ending}";
            if (Parent != null)
            {
                if (Parent.TenantItemId == 0)
                    throw new ApplicationException("The parent class must have a primary key set before calling SetHierarchicalDataKey.");
                DataKey = Parent.DataKey + DataKey;
            }
        }

        public void MoveToNewParent(TenantBase newParent, DbContext context)
        {
            void SetKeyExistingHierarchy(TenantBase existingTenant)
            {
                if (existingTenant.Children == null)
                    context.Entry(existingTenant).Collection(x => x.Children).Load();

                if (!existingTenant.Children.Any())
                    return;
                foreach (var tenant in existingTenant.Children)
                {
                    tenant.SetDataKeyFromHierarchy();
                    SetKeyExistingHierarchy(tenant);
                }
            }

            if (this is Company)
                throw new ApplicationException($"You cannot move a Company.");
            if (newParent == null)
                throw new ApplicationException($"The parent cannot be null.");
            if (newParent.ParentItemId == 0)
                throw new ApplicationException($"The parent {newParent.Name} must be already in the database.");
            if (context.Entry(this).State == EntityState.Detached)
                throw new ApplicationException($"You can't use this method to add a new tenant.");

            LinkToParent(newParent);
            SetDataKeyFromHierarchy();
            //Now change the data key for all the hierarchy from this entry down
            SetKeyExistingHierarchy(this);
        }

        public void MoveToNewParent(int parentId, DbContext context)
        {
            var parent = context.Find<TenantBase>(parentId);
            MoveToNewParent(parent, context);
        }


    }
}