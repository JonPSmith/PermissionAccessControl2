// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonCache;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses;
using PermissionParts;
using Test.EfHelpers;
using Test.FakesAndMocks;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.FeatureAuthorizeTests
{
    public class TestPermissionsChangedFakeDatabase
    {
        [Fact]
        public void TestAddRoleNotTrigger()
        {
            //SETUP
            var fakeCache = new FakeDistributedCache();
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, fakeCache))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                var rolToPer = RoleToPermissions.CreateRoleWithPermissions
                    ("test", "test", new List<Permissions> { Permissions.AccessAll }, context).Result;
                context.Add(rolToPer);
                context.SaveChanges();

                //VERIFY
                fakeCache.CachedKey.ShouldBeNull();
                context.RolesToPermissions.Count().ShouldEqual(1);
            }
        }

        [Fact]
        public void TestUpdateRoleTrigger()
        {
            //SETUP
            var fakeCache = new FakeDistributedCache();
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, fakeCache))
            {
                context.Database.EnsureCreated();
                var rolToPer = RoleToPermissions.CreateRoleWithPermissions
                    ("test", "test", new List<Permissions> { Permissions.AccessAll }, context).Result;
                context.Add(rolToPer);
                context.SaveChanges();

                //ATTEMPT
                rolToPer.UpdatePermissionsInRole(new List<Permissions> { Permissions.EmployeeRead });
                context.SaveChanges();

                //VERIFY
                fakeCache.CachedKey.ShouldNotBeNull();
                context.RolesToPermissions.Count().ShouldEqual(1);
            }
        }

        [Fact]
        public async Task TestAddRoleToUseTrigger()
        {
            //SETUP
            var fakeCache = new FakeDistributedCache();
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, fakeCache))
            {
                context.Database.EnsureCreated();
                var rolToPer = RoleToPermissions.CreateRoleWithPermissions
                    ("test", "test", new List<Permissions> { Permissions.AccessAll }, context).Result;
                context.Add(rolToPer);
                context.SaveChanges();
                fakeCache.CachedKey = null;

                //ATTEMPT
                var userToRole = new UserToRole("test", rolToPer);
                context.Add(userToRole);
                await context.SaveChangesAsync();

                //VERIFY
                fakeCache.CachedKey.ShouldNotBeNull();
                context.UserToRoles.Count().ShouldEqual(1);
            }
        }



    }
}