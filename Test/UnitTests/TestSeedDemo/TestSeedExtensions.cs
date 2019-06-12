// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PermissionAccessControl2.Data;
using ServiceLayer.SeedDemo;
using Test.StartupHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestSeedDemo
{
    public class TestSeedExtensions
    {
        private ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;

        public TestSeedExtensions(ITestOutputHelper output)
        {
            _output = output;
            _serviceProvider = new ConfigureServices().ServiceProvider;
        }

        [Fact]
        public async Task TestSuperAdminSetup()
        {
            //SETUP

            //ATTEMPT
            await _serviceProvider.CheckAddSuperAdminAsync();

            //VERIFY
            using (var extraContext = _serviceProvider.GetRequiredService<ExtraAuthorizeDbContext>())
            {
                var user = await _serviceProvider.GetRequiredService< UserManager<IdentityUser>>().FindByEmailAsync("Super@g1.com");
                user.ShouldNotBeNull();
                extraContext.RolesToPermissions.Single().RoleName.ShouldEqual("SuperAdmin");
                extraContext.UserToRoles.Single().UserId.ShouldEqual(user.Id);
            }
        }

        [Fact]
        public async Task TestSuperAdminSetupTwice()
        {
            //SETUP

            //ATTEMPT
            await _serviceProvider.CheckAddSuperAdminAsync();
            await _serviceProvider.CheckAddSuperAdminAsync();

            //VERIFY
            using (var extraContext = _serviceProvider.GetRequiredService<ExtraAuthorizeDbContext>())
            {
                var user = await _serviceProvider.GetRequiredService<UserManager<IdentityUser>>().FindByEmailAsync("Super@g1.com");
                user.ShouldNotBeNull();
                extraContext.RolesToPermissions.Single().RoleName.ShouldEqual("SuperAdmin");
                extraContext.UserToRoles.Single().UserId.ShouldEqual(user.Id);
            }
        }

        [Fact]
        public async Task TestSeedDataAndUserCheckTenants()
        {
            //SETUP

            //ATTEMPT
            await _serviceProvider.SeedDataAndUserAsync();

            //VERIFY
            var appContext = _serviceProvider.GetRequiredService<AppDbContext>();
            {
                var display = appContext.Tenants.IgnoreQueryFilters().Select(x => x.ToString()).ToList();
                foreach (var line in display)
                {
                    _output.WriteLine($"\"{line}\",");
                }
                display.ShouldEqual(new List<string>
                {
                    "TenantBase: Name = 4U Inc., DataKey = 1|",
                    "TenantBase: Name = West Coast, DataKey = 1|2|",
                    "TenantBase: Name = East Coast, DataKey = 1|3|",
                    "TenantBase: Name = LA Tie4U, DataKey = 1|2|4*",
                    "TenantBase: Name = NY Dress4U, DataKey = 1|3|5*",
                    "TenantBase: Name = Pets2 Ltd., DataKey = 6|",
                    "TenantBase: Name = Cats Place, DataKey = 6|7*",
                    "TenantBase: Name = Kitten Place, DataKey = 6|8*",
                });
            }
        }

        [Fact]
        public async Task TestSeedDataAndUserCheckTenantsTwice()
        {
            //SETUP

            //ATTEMPT
            await _serviceProvider.SeedDataAndUserAsync();
            await _serviceProvider.SeedDataAndUserAsync();

            //VERIFY
            var appContext = _serviceProvider.GetRequiredService<AppDbContext>();
            {
                var display = appContext.Tenants.IgnoreQueryFilters().Select(x => x.ToString()).ToList();
                foreach (var line in display)
                {
                    _output.WriteLine($"\"{line}\",");
                }
                display.ShouldEqual(new List<string>
                {
                    "TenantBase: Name = 4U Inc., DataKey = 1|",
                    "TenantBase: Name = West Coast, DataKey = 1|2|",
                    "TenantBase: Name = East Coast, DataKey = 1|3|",
                    "TenantBase: Name = LA Tie4U, DataKey = 1|2|4*",
                    "TenantBase: Name = NY Dress4U, DataKey = 1|3|5*",
                    "TenantBase: Name = Pets2 Ltd., DataKey = 6|",
                    "TenantBase: Name = Cats Place, DataKey = 6|7*",
                    "TenantBase: Name = Kitten Place, DataKey = 6|8*",
                });
            }
        }

        [Fact]
        public async Task TestSeedDataAndUserCheckUsers()
        {
            //SETUP

            //ATTEMPT
            await _serviceProvider.SeedDataAndUserAsync();

            //VERIFY
            var extraContext = _serviceProvider.GetRequiredService<ExtraAuthorizeDbContext>();
            var appContext = _serviceProvider.GetRequiredService<AppDbContext>();
            {
                extraContext.RolesToPermissions.Select(x => x.RoleName).ToArray()
                    .ShouldEqual(new []
                {
                    "AreaManager", "Director", "SalesAssistant", "StoreManager", "UserAdmin"
                });
                appContext.Tenants.IgnoreQueryFilters().Count().ShouldEqual(8);
            }
            var userContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userEmails = string.Join(", ", userContext.Users.Select(x => x.Email));
            //_output.WriteLine(userEmails);
            userEmails.ShouldEqual("dir@4U.com, westCoast@4U.com, eastCoast@4U.com, "+
                                   "LA-Tie4UBoss@4U.com, NY-Dress4UBoss@4U.com, LA-Tie4USales@4U.com, NY-Dress4USales@4U.com, "+
                                   "dir@Pets2.com, Cats-PlaceSales@Pets2.com, Kitten-PlaceSales@Pets2.com");
        }

        [Fact]
        public async Task TestSeedDataAndUserCheckUsersTwice()
        {
            //SETUP

            //ATTEMPT
            await _serviceProvider.SeedDataAndUserAsync();
            await _serviceProvider.SeedDataAndUserAsync();

            //VERIFY
            var extraContext = _serviceProvider.GetRequiredService<ExtraAuthorizeDbContext>();
            var appContext = _serviceProvider.GetRequiredService<AppDbContext>();
            {
                extraContext.RolesToPermissions.Select(x => x.RoleName).ToArray()
                    .ShouldEqual(new[]
                    {
                        "AreaManager", "Director", "SalesAssistant", "StoreManager", "UserAdmin"
                    });
                appContext.Tenants.IgnoreQueryFilters().Count().ShouldEqual(8);
            }
            var userContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userEmails = string.Join(", ", userContext.Users.Select(x => x.Email));
            //_output.WriteLine(userEmails);
            userEmails.ShouldEqual("dir@4U.com, westCoast@4U.com, eastCoast@4U.com, " +
                                   "LA-Tie4UBoss@4U.com, NY-Dress4UBoss@4U.com, LA-Tie4USales@4U.com, NY-Dress4USales@4U.com, " +
                                   "dir@Pets2.com, Cats-PlaceSales@Pets2.com, Kitten-PlaceSales@Pets2.com");
        }
    }
}