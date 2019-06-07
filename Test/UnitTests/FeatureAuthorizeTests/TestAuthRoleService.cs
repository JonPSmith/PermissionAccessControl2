// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using FeatureAuthorize.UserFeatureServices.Concrete;
using PermissionParts;
using Test.EfHelpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.FeatureAuthorizeTests
{
    public class TestAuthRoleService
    {
        [Theory]
        [InlineData("TestRole1", true)]
        [InlineData("TestRole2", true)]
        [InlineData("BadRole", false)]
        public async Task TestRoleExistsAsync(string roleName, bool expectedResult)
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options))
            {
                context.Database.EnsureCreated();
                var service = new AuthRoleService(context);
                await service.SeedUserWithTwoRolesAsync();

                //ATTEMPT
                var result = await service.RoleExistsAsync(roleName);

                //VERIFY
                result.ShouldEqual(expectedResult);
            }
        }

        [Fact]
        public async Task TestGetRolesForUserAsync()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options))
            {
                context.Database.EnsureCreated();
                var service = new AuthRoleService(context);
                await service.SeedUserWithTwoRolesAsync();

                //ATTEMPT
                var result = await service.GetRolesForUserAsync("userId");

                //VERIFY
                result.Select(x => x.RoleName).ShouldEqual(new []{"TestRole1", "TestRole2"});
            }
        }

        [Fact]
        public async Task TestRemoveRoleFromUserAsync()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options))
            {
                context.Database.EnsureCreated();
                var service = new AuthRoleService(context);
                await service.SeedUserWithTwoRolesAsync();

                //ATTEMPT
                var result = await service.RemoveRoleFromUserAsync("userId", "TestRole2");

                //VERIFY
                (await service.GetRolesForUserAsync("userId")).Select(x => x.RoleName)
                    .ShouldEqual(new[] { "TestRole1" });
            }
        }

        [Fact]
        public async Task TestCreateRoleWithPermissionsAsync()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options))
            {
                context.Database.EnsureCreated();
                var service = new AuthRoleService(context);

                //ATTEMPT
                var status = await service.CreateRoleWithPermissionsAsync("test", 
                    new List<Permissions> {Permissions.ColorCreate});

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                context.RolesToPermissions.Single().PermissionsInRole.ShouldEqual(new List<Permissions> { Permissions.ColorCreate });
            }
        }

        [Fact]
        public async Task TestUpdatePermissionsInRoleAsync()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options))
            {
                context.Database.EnsureCreated();
                var service = new AuthRoleService(context);
                (await service.CreateRoleWithPermissionsAsync("test", new List<Permissions> {Permissions.ColorCreate}))
                    .IsValid.ShouldBeTrue();

                //ATTEMPT
                var status = await service.UpdatePermissionsInRoleAsync("test",
                    new List<Permissions> { Permissions.ColorRead, Permissions.ColorDelete });

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                context.RolesToPermissions.Single().PermissionsInRole
                    .ShouldEqual(new List<Permissions> { Permissions.ColorRead, Permissions.ColorDelete });
            }
        }

        [Fact]
        public async Task TestDeleteRoleAsync()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options))
            {
                context.Database.EnsureCreated();
                var service = new AuthRoleService(context);
                (await service.CreateRoleWithPermissionsAsync("test", new List<Permissions> { Permissions.ColorCreate }))
                    .IsValid.ShouldBeTrue();

                //ATTEMPT
                var status = await service.DeleteRoleAsync("test", false);

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                context.RolesToPermissions.Any().ShouldBeFalse();
            }
        }

        [Fact]
        public async Task TestDeleteRoleAsyncUsedByExistingUser()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options))
            {
                context.Database.EnsureCreated();
                var service = new AuthRoleService(context);
                await service.SeedUserWithTwoRolesAsync();

                //ATTEMPT
                var status = await service.DeleteRoleAsync("TestRole1", true);

                //VERIFY
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                context.RolesToPermissions.Count().ShouldEqual(1);
               (await service.GetRolesForUserAsync("userId")).Select(x => x.RoleName)
                    .ShouldEqual(new[] { "TestRole2" });
            }
        }


    }
}