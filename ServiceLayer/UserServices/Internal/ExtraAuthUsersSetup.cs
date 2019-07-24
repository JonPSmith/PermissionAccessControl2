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

namespace ServiceLayer.UserServices.Internal
{
    /// <summary>
    /// These contain the individual methods to add/update the database, BUT you should call SaveChanges to update the database after using
    /// (This is different to AspNetUserExtension, where the userManger updates the database immediately)
    /// </summary>
    internal class ExtraAuthUsersSetup
    {
        private readonly ExtraAuthorizeDbContext _context;

        public ExtraAuthUsersSetup(ExtraAuthorizeDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// This adds a role if not present, or updates a role if is present.
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="description"></param>
        /// <param name="permissions"></param>
        public void AddUpdateRoleToPermissions(string roleName, string description, ICollection<Permissions> permissions)
        {
            var status = RoleToPermissions.CreateRoleWithPermissions(roleName, description, permissions, _context);
            if (status.IsValid)
                //Note that CreateRoleWithPermissions will return a invalid status if the role is already present.
                _context.Add(status.Result);
            else
            {
                UpdateRole(roleName, description, permissions);
            }
        }

        /// <summary>
        /// This will update a role
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="description"></param>
        /// <param name="permissions"></param>
        public void UpdateRole(string roleName, string description, ICollection<Permissions> permissions)
        {
            var existingRole = _context.Find<RoleToPermissions>(roleName);
            if (existingRole == null)
                throw new KeyNotFoundException($"Could not find the role {roleName} to update.");
            existingRole.Update(description, permissions);
        }

        /// <summary>
        /// This ensures there is a UserToRole linking the userId to the given roleName
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleName"></param>
        public void CheckAddRoleToUser(string userId, string roleName)
        {
            var status = UserToRole.AddRoleToUser(userId, roleName, _context);
            if (status.IsValid)
                //we assume there is already a link to the role is the status wasn't valid
                _context.Add(status.Result);
        }

        /// <summary>
        /// This adds a UserDataHierarchical if not present, or updates the linked tenant if is present.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="linkedTenant"></param>
        public void AddUpdateDataAccessHierarchical(string userId, TenantBase linkedTenant)
        {
            if (linkedTenant == null) throw new ArgumentNullException(nameof(linkedTenant));

            var dataLink = _context.Find<UserDataHierarchical>(userId);
            if (dataLink == null)
            {
                dataLink = new UserDataHierarchical(userId, linkedTenant);
                _context.Add(dataLink);
            }
            else
            {
                dataLink.Update(linkedTenant);
            }
        }

        /// <summary>
        /// This adds if not present a ModulesForUser for a user, using the user's Company's Modules settings.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="linkToTenant"></param>
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