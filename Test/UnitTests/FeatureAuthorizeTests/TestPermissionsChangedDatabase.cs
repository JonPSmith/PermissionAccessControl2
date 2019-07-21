// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonCache;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses;
using Microsoft.EntityFrameworkCore;
using PermissionParts;
using Test.FakesAndMocks;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.FeatureAuthorizeTests
{
    public class TestPermissionsChangedDatabase
    {
        [Fact]
        public void TestAddRoleNotTrigger()
        {
            //SETUP
            var fakeCache = new FakeDistributedCache();
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, new AuthChanges(fakeCache)))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                var rolToPer = RoleToPermissions.CreateRoleWithPermissions
                    ("test", "test", new List<Permissions> { Permissions.AccessAll }, context).Result;
                context.Add(rolToPer);
                context.SaveChanges();

                //VERIFY
                context.TimeStores.Count().ShouldEqual(0);
                context.RolesToPermissions.Count().ShouldEqual(1);
            }
        }

        [Fact]
        public void TestUpdateRoleTrigger()
        {
            //SETUP
            var fakeCache = new FakeDistributedCache();
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, new AuthChanges(fakeCache)))
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
                context.TimeStores.Count().ShouldEqual(1);
                context.RolesToPermissions.Count().ShouldEqual(1);
            }
        }

        [Fact]
        public async Task TestAddRoleToUseTrigger()
        {
            //SETUP
            var fakeCache = new FakeDistributedCache();
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, new AuthChanges(fakeCache)))
            {
                context.Database.EnsureCreated();
                var rolToPer = RoleToPermissions.CreateRoleWithPermissions
                    ("test", "test", new List<Permissions> { Permissions.AccessAll }, context).Result;
                context.Add(rolToPer);
                context.SaveChanges();
                
                //ATTEMPT
                var userToRole = new UserToRole("test", rolToPer);
                context.Add(userToRole);
                await context.SaveChangesAsync();

                //VERIFY
                fakeCache.CachedKey.ShouldNotBeNull();
                context.TimeStores.Count().ShouldEqual(1);
                context.UserToRoles.Count().ShouldEqual(1);
            }
        }

        [Fact]
        public void TestCacheNotUpdatedOnSaveChangesException()
        {
            //SETUP
            var fakeCache = new FakeDistributedCache();
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, new AuthChanges(fakeCache)))
            {
                context.Database.EnsureCreated();
                context.Add(new TimeStore{Key = "dup"});
                context.SaveChanges();
            }
            using (var context = new ExtraAuthorizeDbContext(options, new AuthChanges(fakeCache)))
            {
                context.Database.EnsureCreated();

                var rolToPer = RoleToPermissions.CreateRoleWithPermissions
                    ("test", "test", new List<Permissions> { Permissions.AccessAll }, context).Result;
                context.Add(rolToPer);
                context.SaveChanges();
                fakeCache.CachedKey = null;

                //ATTEMPT
                rolToPer.UpdatePermissionsInRole(new List<Permissions> { Permissions.EmployeeRead });
                context.Add(new TimeStore { Key = "dup" }); //this forces a db exception
                Assert.Throws<DbUpdateException>(() => context.SaveChanges());

                //VERIFY
                fakeCache.CachedKey.ShouldBeNull();
            }
        }

    }
}