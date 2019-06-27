// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using DataLayer.EfCode;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayerTests
{
    public class TestBothDbContexts
    {
        private readonly ITestOutputHelper _output;

        public TestBothDbContexts(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestCreateValidDatabaseOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CombinedDbContext>();
            using (var context = new CombinedDbContext(options))
            {
                //ATTEMPT
                context.Database.EnsureCreated();

                //VERIFY
                var classNames = context.Model.GetEntityTypes().Select(x => x.Name).ToList();
                foreach (var className in classNames)
                {
                    _output.WriteLine($"\"{className}\",");
                }
                classNames.ShouldEqual(new List<string>
                {
                    "DataLayer.ExtraAuthClasses.ModulesForUser",
                    "DataLayer.ExtraAuthClasses.RoleToPermissions",
                    "DataLayer.ExtraAuthClasses.UserDataHierarchical",
                    "DataLayer.ExtraAuthClasses.UserToRole",
                    "DataLayer.MultiTenantClasses.Company",
                    "DataLayer.MultiTenantClasses.RetailOutlet",
                    "DataLayer.MultiTenantClasses.ShopSale",
                    "DataLayer.MultiTenantClasses.ShopStock",
                    "DataLayer.MultiTenantClasses.SubGroup",
                    "DataLayer.MultiTenantClasses.TenantBase",
                });
            }
        }

        
    }
}