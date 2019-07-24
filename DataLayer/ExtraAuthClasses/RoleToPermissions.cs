// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses.Support;
using GenericServices;
using PermissionParts;

namespace DataLayer.ExtraAuthClasses
{
    /// <summary>
    /// This holds each Roles, which are mapped to Permissions
    /// </summary>
    public class RoleToPermissions : IChangeEffectsUser
    {
        [Required(AllowEmptyStrings = false)] //A role must have at least one role in it
        private string _permissionsInRole;

        private RoleToPermissions() { }

        /// <summary>
        /// This creates the Role with its permissions
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="description"></param>
        /// <param name="permissions"></param>
        private RoleToPermissions(string roleName, string description, ICollection<Permissions> permissions)
        {
            RoleName = roleName;
            Update(description, permissions);
        }

        /// <summary>
        /// ShortName of the role
        /// </summary>
        [Key]
        [Required(AllowEmptyStrings = false)]
        [MaxLength(ExtraAuthConstants.RoleNameSize)]
        public string RoleName { get; private set; }

        /// <summary>
        /// A human-friendly description of what the Role does
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public string Description { get; private set; }

        /// <summary>
        /// This returns the list of permissions in this role
        /// </summary>
        public IEnumerable<Permissions> PermissionsInRole => _permissionsInRole.UnpackPermissionsFromString();

        public static IStatusGeneric<RoleToPermissions> CreateRoleWithPermissions(string roleName, string description, ICollection<Permissions> permissionInRole,
            ExtraAuthorizeDbContext context)
        {
            var status = new StatusGenericHandler<RoleToPermissions>();
            if (context.Find<RoleToPermissions>(roleName) != null)
            {
                status.AddError("That role already exists");
                return status;
            }

            return status.SetResult(new RoleToPermissions(roleName, description, permissionInRole));
        }

        public void Update(string description, ICollection<Permissions> permissions)
        {
            if (permissions == null || !permissions.Any())
                throw new InvalidOperationException("There should be at least one permission associated with a role.");

            _permissionsInRole = permissions.PackPermissionsIntoString();
            Description = description;
        }

        public IStatusGeneric DeleteRole(string roleName, bool removeFromUsers,
            ExtraAuthorizeDbContext context)
        {
            var status = new StatusGenericHandler { Message = "Deleted role successfully." };
            var roleToUpdate = context.Find<RoleToPermissions>(roleName);
            if (roleToUpdate == null)
                return status.AddError("That role doesn't exists");

            var usersWithRoles = context.UserToRoles.Where(x => x.RoleName == roleName).ToList();
            if (usersWithRoles.Any())
            {
                if (!removeFromUsers)
                    return status.AddError($"That role is used by {usersWithRoles.Count} and you didn't ask for them to be updated.");

                context.RemoveRange(usersWithRoles);
                status.Message = $"Removed role from {usersWithRoles.Count} user and then deleted role successfully.";
            }

            context.Remove(roleToUpdate);
            return status;
        }


    }
}