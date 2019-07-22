// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.EfCode;
using PermissionParts;
using ServiceLayer.CodeCalledInStartup;
using Test.EfHelpers;
using Test.FakesAndMocks;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.FeatureAuthorizeTests
{
    public class TestCalcAllowedPermissions
    {
        [Fact]
        public async Task TestCalcPermissionsForUserAsync()
        {
            //SETUP
            var fakeAuthChanges = new FakeAuthChanges();
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, fakeAuthChanges))
            {
                context.Database.EnsureCreated();
                context.SeedUserWithDefaultPermissions();

                var calc = new CalcAllowedPermissions(context);
                //ATTEMPT
                var packedP = await calc.CalcPermissionsForUserAsync("userId");

                //VERIFY
                packedP.UnpackPermissionsFromString().ShouldEqual(new List<Permissions> { Permissions.StockRead});
            }
        }

        [Fact]
        public async Task TestCalcPermissionsForUserAsyncWithModule()
        {
            //SETUP
            var fakeAuthChanges = new FakeAuthChanges();
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, fakeAuthChanges))
            {
                context.Database.EnsureCreated();
                context.SeedUserWithDefaultPermissions(PaidForModules.Feature1);

                var calc = new CalcAllowedPermissions(context);
                //ATTEMPT
                var packedP = await calc.CalcPermissionsForUserAsync("userId");

                //VERIFY
                packedP.UnpackPermissionsFromString().ShouldEqual(new List<Permissions> { Permissions.StockRead, Permissions.Feature1Access });
            }
        }

    }
}