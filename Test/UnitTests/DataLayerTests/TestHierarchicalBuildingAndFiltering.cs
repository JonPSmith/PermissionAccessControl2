// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using DataLayer.AppClasses.MultiTenantParts;
using DataLayer.EfCode;
using Test.EfHelpers;
using Test.FakesAndMocks;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataLayerTests
{
    public class TestHierarchicalBuildingAndFiltering
    {
        private ITestOutputHelper _output;

        public TestHierarchicalBuildingAndFiltering(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestCreateCompanyAndChildrenOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<AppDbContext>();
            using (var context = new AppDbContext(options, new FakeGetClaimsProvider("userId", "accessKey")))
            {
                //ATTEMPT
                var rootCompany = context.CreateCompanyAndChildren();
                var display = rootCompany.DisplayHierarchy();

                //VERIFY
                display.ShouldEqual(new List<string>
                {
                    "4U Inc.->West Coast->San Fran->SF Dress4U, DataKey = <null>",
                    "4U Inc.->West Coast->San Fran->SF Tie4U, DataKey = <null>",
                    "4U Inc.->West Coast->San Fran->SF Shirt4U, DataKey = <null>",
                    "4U Inc.->West Coast->LA->LA Dress4U, DataKey = <null>",
                    "4U Inc.->West Coast->LA->LA Tie4U, DataKey = <null>",
                    "4U Inc.->West Coast->LA->LA Shirt4U, DataKey = <null>",
                });
                //foreach (var line in display)
                //{
                //    _output.WriteLine($"\"{line}\",");
                //}
            }
        }

        [Fact]
        public void TestAddCompanyAndChildrenInDatabaseOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<AppDbContext>();
            using (var context = new AppDbContext(options, new FakeGetClaimsProvider("userId", "accessKey")))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                var rootCompany = context.AddCompanyAndChildrenInDatabase();
                var display = rootCompany.DisplayHierarchy();

                //VERIFY
                display.ShouldEqual(new List<string>
                {
                    "4U Inc.->West Coast->San Fran->SF Dress4U, DataKey = 2|4|6*",
                    "4U Inc.->West Coast->San Fran->SF Tie4U, DataKey = 2|4|7*",
                    "4U Inc.->West Coast->San Fran->SF Shirt4U, DataKey = 2|4|8*",
                    "4U Inc.->West Coast->LA->LA Dress4U, DataKey = 3|5|9*",
                    "4U Inc.->West Coast->LA->LA Tie4U, DataKey = 3|5|a*",
                    "4U Inc.->West Coast->LA->LA Shirt4U, DataKey = 3|5|b*",
                });
            }
        }

        [Fact]
        public void TestMarkWithDataKeyIfNeededWorks()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<AppDbContext>();
            using (var context = new AppDbContext(options, new FakeGetClaimsProvider("userId", "123*")))
            {
                context.Database.EnsureCreated();
                var rootCompany = context.AddCompanyAndChildrenInDatabase();

                //ATTEMPT
                

                //VERIFY
                entity.DataKey.ShouldEqual("123*");
            }
        }

        
    }
}