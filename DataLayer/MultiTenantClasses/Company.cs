// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.EfCode;
using PermissionParts;

namespace DataLayer.MultiTenantClasses
{
    /// <summary>
    /// This is the root entry for a multi-tenant entry. There should be only one Company per multi-tenant
    /// </summary>
    public class Company : TenantBase
    {
        private Company(string name) : base(name) { } //Needed by EF Core

        private Company(string name, PaidForModules allowedPaidForModules) : base(name, null)
        {
            AllowedPaidForModules = allowedPaidForModules;
        }

        public static Company AddTenantToDatabaseWithSaveChanges(string name, PaidForModules allowedPaidForModules,
            CompanyDbContext context)
        {
            var newTenant = new Company(name, allowedPaidForModules);
            TenantBase.AddTenantToDatabaseWithSaveChanges(newTenant, context);
            return newTenant;
        }

        /// <summary>
        /// This holds the modules this company have purchased
        /// </summary>
        public PaidForModules AllowedPaidForModules { get; set; }
    }
}