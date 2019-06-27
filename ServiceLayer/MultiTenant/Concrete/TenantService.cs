// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.EfCode;
using DataLayer.MultiTenantClasses;
using Microsoft.EntityFrameworkCore;

namespace ServiceLayer.MultiTenant.Concrete
{
    public class TenantService
    {
        private readonly CompanyDbContext _context;

        public TenantService(CompanyDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// This adds a Company to the database. If it has any children then they will be added as well.
        /// It does this in two stages inside a transaction:
        /// 1) it saves the hierarchy of tenants to the database to get the primary keys
        /// 2) It then sets up the DataKeys in the whole hierarchy
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
                SetKeyInNewHierarchy(rootCompany);
                _context.SaveChanges();

                transaction.Commit();
            }
        }

        public void AddNewTenant(TenantBase newTenant)
        {
            if (newTenant == null) throw new ArgumentNullException(nameof(newTenant));

            if (newTenant is Company)
                throw new ApplicationException($"You should use the {nameof(SetupCompany)} method to add a Company.");
            if (newTenant.Parent == null)
                throw new ApplicationException($"The parent cannot be null.");
            if (newTenant.Parent.ParentItemId == 0)
                throw new ApplicationException($"The parent {newTenant.Parent.Name} must be already in the database.");
            if (_context.Entry(newTenant).State != EntityState.Detached)
                throw new ApplicationException($"You can't use this method to add a tenant that is already in the database.");

            using (var transaction = _context.Database.BeginTransaction())
            {
                // Add this to get primary key set
                _context.Add(newTenant);
                _context.SaveChanges();

                //Now we can set the DataKey
                newTenant.SetDataKeyFromHierarchy();
                _context.SaveChanges();

                transaction.Commit();
            }
        }

        //---------------------------------------------------------
        //private methods

        private static void SetKeyInNewHierarchy(TenantBase tenant)
        {
            if (tenant.Children == null || !tenant.Children.Any())
                return;
            if (tenant is RetailOutlet)
                throw new ApplicationException("Retail outlets cannot have children");
            tenant.SetDataKeyFromHierarchy();
            foreach (var child in tenant.Children)
            {
                child.SetDataKeyFromHierarchy();
                SetKeyInNewHierarchy(child);
            }
        }


    }
}