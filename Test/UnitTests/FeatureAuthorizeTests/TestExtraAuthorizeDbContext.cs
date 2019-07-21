// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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
    public class TestExtraAuthorizeDbContext
    {
        [Fact]
        public void TestSeedUserWithTwoRoles()
        {
            //SETUP
            var fakeAuthChanges = new FakeAuthChanges();
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, fakeAuthChanges))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                context.SeedUserWithTwoRoles();
            }
            using (var context = new ExtraAuthorizeDbContext(options, fakeAuthChanges))
            { 
                //VERIFY
                context.UserToRoles.Select(x => x.RoleName).ToArray().ShouldEqual(new[] { "TestRole1", "TestRole2" });
            }
        }

        [Fact]
        public void TestCreateRoleWithPermissions()
        {
            //SETUP
            var fakeAuthChanges = new FakeAuthChanges();
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, fakeAuthChanges))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                var createStatus = RoleToPermissions.CreateRoleWithPermissions(
                    "test", "test", new List<Permissions> {Permissions.StockAddNew}, context);
                createStatus.IsValid.ShouldBeTrue(createStatus.GetAllErrors());
                context.Add(createStatus.Result);
                context.SaveChanges();
            }
            using (var context = new ExtraAuthorizeDbContext(options, fakeAuthChanges))
            {
                //VERIFY
                context.RolesToPermissions.Single().PermissionsInRole.ShouldEqual(new List<Permissions> { Permissions.StockAddNew });
            }
        }

        [Fact]
        public void TestUpdatePermissionsInRole()
        {
            //SETUP
            var fakeAuthChanges = new FakeAuthChanges();
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, fakeAuthChanges))
            {
                context.Database.EnsureCreated();

                var createStatus = RoleToPermissions.CreateRoleWithPermissions(
                        "test", "test", new List<Permissions> { Permissions.StockRead }, context);
                createStatus.IsValid.ShouldBeTrue(createStatus.GetAllErrors());
                context.Add(createStatus.Result);
                context.SaveChanges();

                //ATTEMPT
                var roleToUpdate = context.Find<RoleToPermissions>("test");
                roleToUpdate.UpdatePermissionsInRole(new List<Permissions> { Permissions.StockAddNew, Permissions.StockRemove });
                context.SaveChanges();
            }
            using (var context = new ExtraAuthorizeDbContext(options, fakeAuthChanges))
            {
                //VERIFY
                context.RolesToPermissions.Single().PermissionsInRole
                    .ShouldEqual(new List<Permissions> { Permissions.StockAddNew, Permissions.StockRemove });
            }
        }

        [Fact]
        public void TestDeleteRole()
        {
            //SETUP
            var fakeAuthChanges = new FakeAuthChanges();
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, fakeAuthChanges))
            {
                context.Database.EnsureCreated();
                var createStatus = RoleToPermissions.CreateRoleWithPermissions(
                    "test", "test", new List<Permissions> { Permissions.StockRead }, context);
                createStatus.IsValid.ShouldBeTrue(createStatus.GetAllErrors());
                context.Add(createStatus.Result);
                context.SaveChanges();

                //ATTEMPT
                var roleToDelete = context.Find<RoleToPermissions>("test");
                context.Remove(roleToDelete);
                context.SaveChanges();

                //VERIFY
                context.RolesToPermissions.Any().ShouldBeFalse();
            }
        }

    }
}