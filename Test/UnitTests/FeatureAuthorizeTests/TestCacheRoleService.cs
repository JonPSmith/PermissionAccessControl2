// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses;
using FeatureAuthorize;
using PermissionParts;
using ServiceLayer.UserServices.Concrete;
using Test.EfHelpers;
using Test.FakesAndMocks;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.FeatureAuthorizeTests
{
    public class TestCacheRoleService
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestSeedCacheRole(bool hasCache2)
        {
            //SETUP
            var fakeAuthChangesFactory = new FakeAuthChangesFactory();
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, fakeAuthChangesFactory))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                context.SeedCacheRole(hasCache2);
            }
            using (var context = new ExtraAuthorizeDbContext(options, fakeAuthChangesFactory))
            { 
                //VERIFY
                var role = context.RolesToPermissions.Single();
                role.RoleName.ShouldEqual("CacheRole");
                role.PermissionsInRole.Count().ShouldEqual(hasCache2 ? 2 : 1);
            }
        }

        [Fact]
        public void TestToggleCacheRole()
        {
            //SETUP
            var fakeAuthChangesFactory = new FakeAuthChangesFactory();
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, fakeAuthChangesFactory))
            {
                context.Database.EnsureCreated();
                context.SeedCacheRole(true);

                var cacheRoleService = new CacheRoleService(context);
                var claims = new List<Claim>
                {
                    new Claim(PermissionConstants.PackedPermissionClaimType,
                        new List<Permissions> {Permissions.Cache1, Permissions.Cache2}.PackPermissionsIntoString())
                };

                //ATTEMPT
                cacheRoleService.ToggleCacheRole(claims);

                //VERIFY
                var role = context.RolesToPermissions.Single();
                role.RoleName.ShouldEqual("CacheRole");
                role.PermissionsInRole.Count().ShouldEqual(1);
                fakeAuthChangesFactory.FakeAuthChanges.CacheValueSet.ShouldBeTrue();
            }
        }

    }
}