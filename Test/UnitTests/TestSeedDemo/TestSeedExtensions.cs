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
using PermissionParts;
using ServiceLayer.SeedDemo;
using Test.DiConfigHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.TestSeedDemo
{
    public class TestSeedExtensions
    {
        private readonly ITestOutputHelper _output;
        private readonly ServiceProvider _serviceProvider;

        public TestSeedExtensions(ITestOutputHelper output)
        {
            _output = output;
            _serviceProvider = this.SetupServices();
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
            await _serviceProvider.CheckSeedDataAndUserAsync();

            //VERIFY
            var companyContext = _serviceProvider.GetRequiredService<CompanyDbContext>();
            {
                var display = companyContext.Tenants.IgnoreQueryFilters().Select(x => x.ToString()).ToList();
                foreach (var line in display)
                {
                    _output.WriteLine($"\"{line}\",");
                }
                display.ShouldEqual(new List<string>
                {
                    "Company: Name = 4U Inc., DataKey = 1|",
                    "SubGroup: Name = West Coast, DataKey = 1|2|",
                    "SubGroup: Name = East Coast, DataKey = 1|3|",
                    "RetailOutlet: Name = LA Tie4U, DataKey = 1|2|4*",
                    "RetailOutlet: Name = NY Dress4U, DataKey = 1|3|5*",
                    "Company: Name = Pets2 Ltd., DataKey = 6|",
                    "RetailOutlet: Name = Cats Place, DataKey = 6|7*",
                    "RetailOutlet: Name = Kitten Place, DataKey = 6|8*",
                });
            }
        }

        [Fact]
        public async Task TestSeedDataAndUserCheckTenantsTwice()
        {
            //SETUP

            //ATTEMPT
            await _serviceProvider.CheckSeedDataAndUserAsync();
            await _serviceProvider.CheckSeedDataAndUserAsync();

            //VERIFY
            var companyContext = _serviceProvider.GetRequiredService<CompanyDbContext>();
            {
                var display = companyContext.Tenants.IgnoreQueryFilters().Select(x => x.ToString()).ToList();
                foreach (var line in display)
                {
                    _output.WriteLine($"\"{line}\",");
                }
                display.ShouldEqual(new List<string>
                {
                    "Company: Name = 4U Inc., DataKey = 1|",
                    "SubGroup: Name = West Coast, DataKey = 1|2|",
                    "SubGroup: Name = East Coast, DataKey = 1|3|",
                    "RetailOutlet: Name = LA Tie4U, DataKey = 1|2|4*",
                    "RetailOutlet: Name = NY Dress4U, DataKey = 1|3|5*",
                    "Company: Name = Pets2 Ltd., DataKey = 6|",
                    "RetailOutlet: Name = Cats Place, DataKey = 6|7*",
                    "RetailOutlet: Name = Kitten Place, DataKey = 6|8*",
                });
            }
        }

        [Fact]
        public async Task TestSeedDataAndUserCheckUsers()
        {
            //SETUP

            //ATTEMPT
            await _serviceProvider.CheckSeedDataAndUserAsync();

            //VERIFY
            var extraContext = _serviceProvider.GetRequiredService<ExtraAuthorizeDbContext>();
            var companyContext = _serviceProvider.GetRequiredService<CompanyDbContext>();
            {
                extraContext.RolesToPermissions.Select(x => x.RoleName).ToArray()
                    .ShouldEqual(new []
                {
                    "AreaManager", "Director", "SalesAssistant", "StoreManager", "UserAdmin"
                });
                companyContext.Tenants.IgnoreQueryFilters().Count().ShouldEqual(8);
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
            await _serviceProvider.CheckSeedDataAndUserAsync();
            await _serviceProvider.CheckSeedDataAndUserAsync();

            //VERIFY
            var extraContext = _serviceProvider.GetRequiredService<ExtraAuthorizeDbContext>();
            var companyContext = _serviceProvider.GetRequiredService<CompanyDbContext>();
            {
                extraContext.RolesToPermissions.Select(x => x.RoleName).ToArray()
                    .ShouldEqual(new[]
                    {
                        "AreaManager", "Director", "SalesAssistant", "StoreManager", "UserAdmin"
                    });
                companyContext.Tenants.IgnoreQueryFilters().Count().ShouldEqual(8);
            }
            var userContext = _serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userEmails = string.Join(", ", userContext.Users.Select(x => x.Email));
            //_output.WriteLine(userEmails);
            userEmails.ShouldEqual("dir@4U.com, westCoast@4U.com, eastCoast@4U.com, " +
                                   "LA-Tie4UBoss@4U.com, NY-Dress4UBoss@4U.com, LA-Tie4USales@4U.com, NY-Dress4USales@4U.com, " +
                                   "dir@Pets2.com, Cats-PlaceSales@Pets2.com, Kitten-PlaceSales@Pets2.com");
        }

        [Fact]
        public async Task TestSeedDatAddModulesToUser()
        {
            //SETUP

            //ATTEMPT
            await _serviceProvider.CheckSeedDataAndUserAsync();

            //VERIFY
            var extraContext = _serviceProvider.GetRequiredService<ExtraAuthorizeDbContext>();
            var numUsers = _serviceProvider.GetRequiredService<UserManager<IdentityUser>>().Users.Count();
            extraContext.ModulesForUsers.Count().ShouldEqual(numUsers);
            extraContext.ModulesForUsers.All(x => x.AllowedPaidForModules == PaidForModules.None).ShouldBeTrue();
        }
    }
}