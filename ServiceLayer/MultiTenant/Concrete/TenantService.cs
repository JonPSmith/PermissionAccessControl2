// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using DataLayer.AppClasses.MultiTenantParts;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;

namespace ServiceLayer.MultiTenant.Concrete
{
    public class TenantService
    {
        private readonly AppDbContext _context;

        public TenantService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// This takes a Company and a series of children and adds it to the database. (Useful for adding all the ten
        /// It does this in two stages inside a transaction:
        /// 1) it saves the hierarchy of tenants to the database to get the primary keys
        /// </summary>
        /// <param name="rootCompany"></param>
        public void SetupCompany(Company rootCompany)
        {
            if (rootCompany == null) throw new ArgumentNullException(nameof(rootCompany));

            using (var transaction = _context.Database.BeginTransaction())
            {
                // This gets the whole hierarchy into the database, and their primary keys set
                _context.Add(rootCompany);
                _context.SaveChanges();
                
                //Now we can set the DataKey
                rootCompany.SetAccessKey();
                SetKeyInNewHierarchy(rootCompany.Children);
                _context.SaveChanges();

                transaction.Commit();
            }
        }

        public void AddTenantToParent(TenantBase newTenant, TenantBase parent)
        {
            if (newTenant == null) throw new ArgumentNullException(nameof(newTenant));

            if (newTenant is Company)
                throw new ApplicationException($"You should use the {nameof(SetupCompany)} method to add a Company.");
            if (parent == null)
                throw new ApplicationException($"The parent cannot be null.");
            if (parent.ParentItemId == 0)
                throw new ApplicationException($"The parent {parent.Name} must be already in the database.");
            if (_context.Entry(newTenant).State != EntityState.Detached)
                throw new ApplicationException($"You can't use this method to add a tenant that is already in the database.");

            using (var transaction = _context.Database.BeginTransaction())
            {
                // This gets the whole hierarchy into the database, and their primary keys set
                newTenant.LinkToParent(parent);
                _context.SaveChanges();

                //Now we can set the DataKey
                newTenant.SetAccessKey();
                _context.SaveChanges();

                transaction.Commit();
            }
        }

        public void MoveTenantToNewParent(TenantBase existingTenant, TenantBase newParent)
        {
            if (existingTenant == null) throw new ArgumentNullException(nameof(existingTenant));

            if (existingTenant is Company)
                throw new ApplicationException($"You should use the {nameof(SetupCompany)} method to add a Company.");
            if (newParent == null)
                throw new ApplicationException($"The parent cannot be null.");
            if (newParent.ParentItemId == 0)
                throw new ApplicationException($"The parent {newParent.Name} must be already in the database.");
            if (_context.Entry(existingTenant).State == EntityState.Detached)
                throw new ApplicationException($"You can't use this method to add a new tenant.");

            existingTenant.LinkToParent(newParent);
            existingTenant.SetAccessKey();
            //Now change the data key for all the hierarchy from this entry down
            SetKeyExistingHierarchy(existingTenant);
            _context.SaveChanges();
        }

        //---------------------------------------------------------
        //private methods

        private static void SetKeyInNewHierarchy(ICollection<TenantBase> children)
        {
            if (children == null || !children.Any())
                return;
            foreach (var tenant in children)
            {
                tenant.SetAccessKey();
                SetKeyInNewHierarchy(tenant.Children);
            }
        }

        private  void SetKeyExistingHierarchy(TenantBase existingTenant)
        {
            if (existingTenant.Children == null)
                _context.Entry(existingTenant).Collection(x => x.Children).Load();

            if (!existingTenant.Children.Any())
                return;
            foreach (var tenant in existingTenant.Children)
            {
                tenant.SetAccessKey();
                SetKeyInNewHierarchy(tenant.Children);
            }
        }
    }
}