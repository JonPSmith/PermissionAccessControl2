// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses;
using PermissionParts;
using Xunit.Extensions.AssertExtensions;

namespace Test.EfHelpers
{
    public static class AuthRoleServiceHelpers
    {
        public static void SeedUserWithTwoRoles(this ExtraAuthorizeDbContext context, string userId = "userId")
        {
            var userStatus = RoleToPermissions.CreateRoleWithPermissions(
                "TestRole1", new List<Permissions> { Permissions.ColorRead}, context);
            userStatus.IsValid.ShouldBeTrue(userStatus.GetAllErrors());
            context.Add(userStatus.Result);
            userStatus = RoleToPermissions.CreateRoleWithPermissions(
                "TestRole2", new List<Permissions> { Permissions.ColorDelete }, context);
            userStatus.IsValid.ShouldBeTrue(userStatus.GetAllErrors());
            context.Add(userStatus.Result);

            var roleStatus = UserToRole.AddRoleToUser(userId, "TestRole1", context);
            roleStatus.IsValid.ShouldBeTrue(userStatus.GetAllErrors());
            context.Add(roleStatus.Result);
            roleStatus = UserToRole.AddRoleToUser(userId, "TestRole2", context);
            roleStatus.IsValid.ShouldBeTrue(userStatus.GetAllErrors());
            context.Add(roleStatus.Result);

            context.SaveChanges();
        }
    }
}