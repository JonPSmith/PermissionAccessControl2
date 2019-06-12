// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        public async Task TestSeedDataAndUser()
        {
            //SETUP

            //ATTEMPT
            await _serviceProvider.SeedDataAndUserAsync();

            //VERIFY
            var extraContext = _serviceProvider.GetRequiredService<ExtraAuthorizeDbContext>();
            var appContext = _serviceProvider.GetRequiredService<AppDbContext>();
            {
                var roles = extraContext.RolesToPermissions.ToList();
                var userToRoles = extraContext.UserToRoles.ToList();
                var tenants = appContext.Tenants.IgnoreQueryFilters().ToList();
            }
        }
    }
}