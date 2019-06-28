// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.EfCode;
using PermissionParts;

namespace DataLayer.MultiTenantClasses
{
    public class SubGroup : TenantBase
    {
        private SubGroup(string name) : base(name) { } //Needed by EF Core

        private SubGroup(string name, TenantBase parent) : base(name, parent)
        {
        }

        public static SubGroup AddTenantToDatabaseWithSaveChanges(string name, TenantBase parent, CompanyDbContext context)
        {
            var newTenant = new SubGroup(name, parent);
            TenantBase.AddTenantToDatabaseWithSaveChanges(newTenant, context);
            return newTenant;
        }
    }
}