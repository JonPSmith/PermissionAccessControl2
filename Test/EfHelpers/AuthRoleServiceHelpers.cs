// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using FeatureAuthorize.UserFeatureServices.Concrete;
using PermissionParts;
using Xunit.Extensions.AssertExtensions;

namespace Test.EfHelpers
{
    public static class AuthRoleServiceHelpers
    {
        public static async Task SeedUserWithTwoRolesAsync(this AuthRoleService service, string userId = "userId")
        {
            var userStatus = await service.CreateRoleWithPermissionsAsync("TestRole1", new List<Permissions> { Permissions.ColorRead});
            userStatus.IsValid.ShouldBeTrue(userStatus.GetAllErrors());
            userStatus = await service.CreateRoleWithPermissionsAsync("TestRole2", new List<Permissions> { Permissions.ColorDelete });
            userStatus.IsValid.ShouldBeTrue(userStatus.GetAllErrors());

            userStatus = await service.AddRoleToUserAsync(userId, "TestRole1");
            userStatus.IsValid.ShouldBeTrue(userStatus.GetAllErrors());
            userStatus = await service.AddRoleToUserAsync(userId, "TestRole2");
            userStatus.IsValid.ShouldBeTrue(userStatus.GetAllErrors());
        }
    }
}