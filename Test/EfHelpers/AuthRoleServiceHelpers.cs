// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses;
using PermissionParts;
using Xunit.Extensions.AssertExtensions;

namespace Test.EfHelpers
{
    public static class AuthRoleServiceHelpers
    {
        public static void SeedUserWithDefaultPermissions(this ExtraAuthorizeDbContext context,
            PaidForModules modules = PaidForModules.None, string userId = "userId")
        {
            var defaultPermissions = new List<Permissions> {Permissions.StockRead, Permissions.Feature1Access};

            var roleStatus = RoleToPermissions.CreateRoleWithPermissions(
                "TestRole1", "TestRole1", defaultPermissions, context);
            roleStatus.IsValid.ShouldBeTrue(roleStatus.GetAllErrors());
            context.Add(roleStatus.Result);

            var moduleUser = new ModulesForUser(userId, modules);
            context.Add(moduleUser);

            var userStatus = UserToRole.AddRoleToUser(userId, "TestRole1", context);
            userStatus.IsValid.ShouldBeTrue(roleStatus.GetAllErrors());
            context.Add(userStatus.Result);

            context.SaveChanges();
        }

        public static void SeedUserWithTwoRoles(this ExtraAuthorizeDbContext context, string userId = "userId")
        {
            var userStatus = RoleToPermissions.CreateRoleWithPermissions(
                "TestRole1", "TestRole1", new List<Permissions> { Permissions.StockRead}, context);
            userStatus.IsValid.ShouldBeTrue(userStatus.GetAllErrors());
            context.Add(userStatus.Result);
            userStatus = RoleToPermissions.CreateRoleWithPermissions(
                "TestRole2", "TestRole1", new List<Permissions> { Permissions.SalesSell}, context);
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

        public static void SeedCacheRole(this ExtraAuthorizeDbContext context, bool hasCache2, string userId = "userId")
        {
            var permissions = new List<Permissions> { Permissions.Cache1 };
            if (hasCache2)
                permissions.Add(Permissions.Cache2);
            var userStatus = RoleToPermissions.CreateRoleWithPermissions(
                "CacheRole", "CacheRole", permissions, context);
            userStatus.IsValid.ShouldBeTrue(userStatus.GetAllErrors());
            context.Add(userStatus.Result);

            context.SaveChanges();
        }
    }
}