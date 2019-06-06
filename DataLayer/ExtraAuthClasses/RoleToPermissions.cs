// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using PermissionParts;

namespace DataLayer.ExtraAuthClasses
{
    public class RoleToPermissions
    {
        [Required(AllowEmptyStrings = false)] //A role must have at least one role in it
        private string _permissionsInRole;

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

        private RoleToPermissions() { }

        /// <summary>
        /// This creates the Role with its permissions
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="description"></param>
        /// <param name="permissions"></param>
        public RoleToPermissions(string roleName, string description, ICollection<Permissions> permissions)
        {
            RoleName = roleName;
            Update(description, permissions);
        }

        public void Update(string description, ICollection<Permissions> permissions)
        {
            Description = description;
            if (permissions == null || !permissions.Any())
                throw new InvalidOperationException("There should be at least one permission associated with a role.");

            _permissionsInRole = permissions.PackPermissionsIntoString();
        }
    }
}