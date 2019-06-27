// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses;
using DataLayer.MultiTenantClasses;
using PermissionParts;

[assembly: InternalsVisibleTo("Test")]

namespace ServiceLayer.SeedDemo.Internal
{
    /// <summary>
    /// These contain the individual methods to add/update the database, BUT you should call SaveChanges to update the database
    /// (This is different to AspNetUserExtension, where the userManger updates the database (immediately)
    /// </summary>
    internal class SetupExtraAuthUsers
    {
        private readonly ExtraAuthorizeDbContext _context;

        public SetupExtraAuthUsers(ExtraAuthorizeDbContext context)
        {
            _context = context;
        }

        public void CheckAddNewRole(string roleName, string description, ICollection<Permissions> permissions)
        {
            var status = RoleToPermissions.CreateRoleWithPermissions(roleName, description, permissions, _context);
            if (status.IsValid)
                //Note that CreateRoleWithPermissions will return a invalid status if the role is already present.
                _context.Add(status.Result);
        }

        public void CheckAddRoleToUser(string userId, string roleName)
        {
            var status = UserToRole.AddRoleToUser(userId, roleName, _context);
            if (status.IsValid)
                //we assume there is already a link to the role is the status wasn't valid
                _context.Add(status.Result);
        }

        public void CheckAddDataAccessHierarchical(string userId, TenantBase linkedTenant)
        {
            if (linkedTenant == null) throw new ArgumentNullException(nameof(linkedTenant));

            if (_context.Find<UserDataHierarchical>(userId) == null)
            {
                var dataAccess = new UserDataHierarchical(userId, linkedTenant);
                _context.Add(dataAccess);
            }
        }

        public void CheckAddModules(string userId, TenantBase linkToTenant)
        {
            if (_context.Find<ModulesForUser>(userId) == null)
            {
                var company = _context.Find<Company>(linkToTenant.ExtractCompanyId());
                if (company == null)
                    throw new NullReferenceException($"Could not find the company with primary key of {linkToTenant.ExtractCompanyId()}.");
                var dataAccess = new ModulesForUser(userId, company.AllowedPaidForModules);
                _context.Add(dataAccess);
            }
        }


    }
}