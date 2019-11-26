﻿// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses.Support;
using GenericServices;

namespace DataLayer.ExtraAuthClasses
{
    /// <summary>
    /// This is a one-to-many relationship between the User (represented by the UserId) and their Roles (represented by RoleToPermissions)
    /// </summary>
    public class UserToRole : IAddRemoveEffectsUser, IChangeEffectsUser
    {
        private UserToRole() { } //needed by EF Core

        public UserToRole(string userId, RoleToPermissions role)
        {
            UserId = userId;
            Role = role;
        }

        //I use a composite key for this table: combination of UserId and RoleName
        //That has to be defined by EF Core's fluent API
        [Required(AllowEmptyStrings = false)]
        [MaxLength(ExtraAuthConstants.UserIdSize)] 
        public string UserId { get; private set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(ExtraAuthConstants.RoleNameSize)]
        public string RoleName { get; private set; }

        [ForeignKey(nameof(RoleName))]
        public RoleToPermissions Role { get; private set; }


        public static IStatusGeneric<UserToRole> AddRoleToUser(string userId, string roleName, ExtraAuthorizeDbContext context)
        {
            if (userId == null) throw new ArgumentNullException(nameof(userId));
            if (roleName == null) throw new ArgumentNullException(nameof(roleName));

            var status = new StatusGenericHandler<UserToRole>();
            if (context.Find<UserToRole>(userId, roleName) != null)
            {
                status.AddError($"The user already has the Role '{roleName}'.");
                return status;
            }
            var roleToAdd = context.Find<RoleToPermissions>(roleName);
            if (roleToAdd == null)
            {
                status.AddError($"I could not find the Role '{roleName}'.");
                return status;
            }

            return status.SetResult(new UserToRole(userId, roleToAdd));
        }
    }
}