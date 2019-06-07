// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses;
using GenericServices;
using Microsoft.EntityFrameworkCore;
using PermissionParts;

namespace FeatureAuthorize.UserFeatureServices.Concrete
{
    public class AuthRoleService
    {
        private readonly ExtraAuthorizeDbContext _context;

        public AuthRoleService(ExtraAuthorizeDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _context.RolesToPermissions.AnyAsync(x => x.RoleName == roleName);
        }

        public async Task<IStatusGeneric> CreateRoleWithPermissionsAsync(string roleName, 
            ICollection<Permissions> permissionInRole)
        {
            var status = new StatusGenericHandler();
            if (await _context.FindAsync<RoleToPermissions>(roleName) != null)
                return status.AddError("That role already exists");

            _context.Add(new RoleToPermissions(roleName, permissionInRole));
            await _context.SaveChangesAsync();
            return status;
        }

        public async Task<IStatusGeneric> UpdatePermissionsInRoleAsync(string roleName,
            ICollection<Permissions> permissionInRole)
        {
            var status = new StatusGenericHandler();
            var roleToUpdate = await _context.FindAsync<RoleToPermissions>(roleName);
            if (roleToUpdate == null)
                return status.AddError("That role doesn't exists");

            roleToUpdate.Update(permissionInRole);
            await _context.SaveChangesAsync();
            return status;
        }

        public async Task<IStatusGeneric> DeleteRoleAsync(string roleName, bool removeFromUsers)
        {
            var status = new StatusGenericHandler {Message = "Deleted role successfully."};
            var roleToUpdate = await _context.FindAsync<RoleToPermissions>(roleName);
            if (roleToUpdate == null)
                return status.AddError("That role doesn't exists");

            var usersWithRoles = await _context.UserToRoles.Where(x => x.RoleName == roleName).ToListAsync();
            if (usersWithRoles.Any())
            {
                if (!removeFromUsers)
                    return status.AddError($"That role is used by {usersWithRoles.Count} and you didn't ask for them to be updated.");
                foreach (var usersWithRole in usersWithRoles)
                {
                    status.CombineStatuses(await RemoveRoleFromUserNoSaveChangesAsync(usersWithRole.UserId, roleName));
                    if (!status.IsValid)
                        return status;
                }

                status.Message = $"Removed role from {usersWithRoles.Count} user and then deleted role successfully.";
            }

            _context.Remove(roleToUpdate);
            await _context.SaveChangesAsync();
            return status;
        }

        public async Task<IStatusGeneric> AddRoleToUserAsync(string userId, string roleName)
        {
            if (roleName == null) throw new ArgumentNullException(nameof(roleName));

            var status = new StatusGenericHandler();
            var roleToAdd = await _context.FindAsync<RoleToPermissions>(roleName);
            if (roleToAdd == null)
                return status.AddError($"I could not find the role {roleName}.");

            _context.Add(new UserToRole(userId, roleToAdd));
            await _context.SaveChangesAsync();

            return status;
        }

        public async Task<IStatusGeneric> RemoveRoleFromUserAsync(string userId, string roleName)
        {
            var status = await RemoveRoleFromUserNoSaveChangesAsync(userId, roleName);
            if (status.IsValid)
                await _context.SaveChangesAsync();

            return status;
        }

        public async Task<ICollection<RoleToPermissions>> GetRolesForUserAsync(string userId)
        {
            return await _context.UserToRoles.Where(x => x.UserId == userId).Select(x => x.Role).ToListAsync();
        }

        //----------------------------------------------------------------
        //private methods 

        public async Task<IStatusGeneric> RemoveRoleFromUserNoSaveChangesAsync(string userId, string roleName)
        {
            if (roleName == null) throw new ArgumentNullException(nameof(roleName));

            var status = new StatusGenericHandler();
            var roleToRemove = await _context.FindAsync<UserToRole>(userId, roleName);
            if (roleToRemove == null)
                return status.AddError($"The user does not have the role {roleName}.");

            _context.Remove(roleToRemove);

            return status;
        }

    }
}