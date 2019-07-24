// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using DataLayer.EfCode;
using DataLayer.MultiTenantClasses;
using Microsoft.EntityFrameworkCore;
using PermissionParts;
using ServiceLayer.CodeCalledInStartup;
using ServiceLayer.SeedDemo.Internal;
using Test.FakesAndMocks;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataAuthorizeTests
{
    public class TestHierarchicalBuildingChanging
    {
        private readonly ITestOutputHelper _output;

        public TestHierarchicalBuildingChanging(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestCreateCompany()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider(null)))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                var company = Company.AddTenantToDatabaseWithSaveChanges("TestCompany", PaidForModules.None, context);

                //VERIFY
                company.DataKey.ShouldNotBeNull();
            }
        }

        [Fact]
        public void TestCreateCompanyAndChild()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider(null)))
            {
                context.Database.EnsureCreated();
                var company = Company.AddTenantToDatabaseWithSaveChanges("TestCompany", PaidForModules.None, context);

                //ATTEMPT
                var shop = RetailOutlet.AddTenantToDatabaseWithSaveChanges("TestShop", company, context);

                //VERIFY
                shop.DataKey.ShouldNotBeNull();
            }
        }

        [Fact]
        public void TestAddCompanyAndChildrenInDatabaseOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("accessKey")))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                context.AddCompanyAndChildrenInDatabase();

                //VERIFY
                var display = context.Tenants.IgnoreQueryFilters().Select(x => x.ToString()).ToList();
                foreach (var line in display)
                {
                    _output.WriteLine($"\"{line}\",");
                }
                display.ShouldEqual(new List<string>
                {
                    "Company: Name = 4U Inc., DataKey = 1|",
                    "SubGroup: Name = West Coast, DataKey = 1|2|",
                    "SubGroup: Name = San Fran, DataKey = 1|2|3|",
                    "RetailOutlet: Name = SF Dress4U, DataKey = 1|2|3|4*",
                    "RetailOutlet: Name = SF Tie4U, DataKey = 1|2|3|5*",
                    "RetailOutlet: Name = SF Shirt4U, DataKey = 1|2|3|6*",
                    "SubGroup: Name = LA, DataKey = 1|2|7|",
                    "RetailOutlet: Name = LA Dress4U, DataKey = 1|2|7|8*",
                    "RetailOutlet: Name = LA Tie4U, DataKey = 1|2|7|9*",
                    "RetailOutlet: Name = LA Shirt4U, DataKey = 1|2|7|a*",
                });
            }
        }

        [Fact]
        public void TestIncludeIsAffectedByQueryFilterOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            string dataKey;
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("accessKey")))
            {
                context.Database.EnsureCreated();
                var companies = context.AddCompanyAndChildrenInDatabase();
                dataKey = companies.First().Children.First().Children.First().DataKey;
            }
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider(dataKey)))
            {
                //ATTEMPT
                var tenant = context.Tenants
                    .Include(p => p.Parent)
                    .Include(x => x.Children).First();

                //VERIFY
                tenant.DataKey.ShouldEqual(dataKey);
                tenant.Parent.ShouldBeNull();
                tenant.Children.Any().ShouldBeTrue();
            }
        }

        [Fact]
        public void TestIncludeWithNoQueryFilterOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            string dataKey;
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("accessKey")))
            {
                context.Database.EnsureCreated();
                var companies = context.AddCompanyAndChildrenInDatabase();
                dataKey = companies.First().Children.First().Children.First().DataKey;
            }
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider(dataKey)))
            {
                //ATTEMPT
                var tenant = context.Tenants.IgnoreQueryFilters()
                    .Include(p => p.Parent)
                    .Include(x => x.Children).Single(x => x.DataKey == dataKey);

                //VERIFY
                tenant.DataKey.ShouldEqual(dataKey);
                tenant.Parent.ShouldNotBeNull();
                tenant.Children.Any().ShouldBeTrue();
            }
        }

        [Fact]
        public void TestAddCompanyAndChildrenInDatabaseTwoCompaniesOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("accessKey")))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                context.AddCompanyAndChildrenInDatabase(
                    "Company1|Area|Shop1", "Company1|Area|Shop2", "Company2|Area|Shop3");

                //VERIFY
                var display = context.Tenants.IgnoreQueryFilters().Select(x => x.ToString()).ToList();
                foreach (var line in display)
                {
                    _output.WriteLine($"\"{line}\",");
                }
                display.ShouldEqual(new List<string>
                {
                    "Company: Name = Company1, DataKey = 1|",
                    "SubGroup: Name = Area, DataKey = 1|2|",
                    "RetailOutlet: Name = Shop1, DataKey = 1|2|3*",
                    "RetailOutlet: Name = Shop2, DataKey = 1|2|4*",
                    "Company: Name = Company2, DataKey = 5|",
                    "SubGroup: Name = Area, DataKey = 5|6|",
                    "RetailOutlet: Name = Shop3, DataKey = 5|6|7*",
                });
            }
        }

        [Fact]
        public void TestExtractCompanyId()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("123*")))
            {
                context.Database.EnsureCreated();
                var rootCompanies = context.AddCompanyAndChildrenInDatabase();

                //ATTEMPT
                //                   -- West Coast --|--- San Fran ---
                var sfDress4U = rootCompanies.First().Children.Single().Children.First().Children.First();
                //                  -- West Coast --|------ LA ------
                var la = rootCompanies.First().Children.Single().Children.Last();


                //VERIFY
                rootCompanies.First().ExtractCompanyId().ShouldEqual(1);
                sfDress4U.ExtractCompanyId().ShouldEqual(1);
                la.ExtractCompanyId().ShouldEqual(1);
            }
        }

        [Fact]
        public void TestMoveToNewParentSimple()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("123*")))
            {
                context.Database.EnsureCreated();
                var rootCompanies = context.AddCompanyAndChildrenInDatabase();
                //                          -- West Coast --|--- San Fran ---|-- SF Dress4U --
                var sfDress4U = rootCompanies.First().Children.Single().Children.First().Children.First();
                //                          - West Coast --
                var westCoast = rootCompanies.First().Children.Single();

                //ATTEMPT
                sfDress4U.MoveToNewParent(westCoast, context);
                context.SaveChanges();

                //VERIFY
                var display = context.Tenants.IgnoreQueryFilters().Select(x => x.ToString()).ToList();
                foreach (var line in display)
                {
                    _output.WriteLine($"\"{line}\",");
                }
                display.ShouldEqual(new List<string>
                {
                    "Company: Name = 4U Inc., DataKey = 1|",
                    "SubGroup: Name = West Coast, DataKey = 1|2|",
                    "SubGroup: Name = San Fran, DataKey = 1|2|3|",
                    "RetailOutlet: Name = SF Dress4U, DataKey = 1|2|4*",
                    "RetailOutlet: Name = SF Tie4U, DataKey = 1|2|3|5*",
                    "RetailOutlet: Name = SF Shirt4U, DataKey = 1|2|3|6*",
                    "SubGroup: Name = LA, DataKey = 1|2|7|",
                    "RetailOutlet: Name = LA Dress4U, DataKey = 1|2|7|8*",
                    "RetailOutlet: Name = LA Tie4U, DataKey = 1|2|7|9*",
                    "RetailOutlet: Name = LA Shirt4U, DataKey = 1|2|7|a*",
                });

            }
        }

        [Fact]
        public void TestMoveToNewParentGroup()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("123*")))
            {
                context.Database.EnsureCreated();
                var rootCompanies = context.AddCompanyAndChildrenInDatabase();
                //                   -- West Coast --|--- San Fran ---
                var sanFran = rootCompanies.First().Children.Single().Children.First();
                //                  -- West Coast --|------ LA ------
                var la = rootCompanies.First().Children.Single().Children.Last();

                //ATTEMPT
                sanFran.MoveToNewParent(la, context);
                context.SaveChanges();

                //VERIFY
                var display = context.Tenants.IgnoreQueryFilters().Select(x => x.ToString()).ToList();
                foreach (var line in display)
                {
                    _output.WriteLine($"\"{line}\",");
                }
                display.ShouldEqual(new List<string>
                {
                    "Company: Name = 4U Inc., DataKey = 1|",
                    "SubGroup: Name = West Coast, DataKey = 1|2|",
                    "SubGroup: Name = San Fran, DataKey = 1|2|7|3|",
                    "RetailOutlet: Name = SF Dress4U, DataKey = 1|2|7|3|4*",
                    "RetailOutlet: Name = SF Tie4U, DataKey = 1|2|7|3|5*",
                    "RetailOutlet: Name = SF Shirt4U, DataKey = 1|2|7|3|6*",
                    "SubGroup: Name = LA, DataKey = 1|2|7|",
                    "RetailOutlet: Name = LA Dress4U, DataKey = 1|2|7|8*",
                    "RetailOutlet: Name = LA Tie4U, DataKey = 1|2|7|9*",
                    "RetailOutlet: Name = LA Shirt4U, DataKey = 1|2|7|a*",
                });
            }
        }

        [Fact]
        public void TestAddTenantRetailOutletOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("accessKey")))
            {
                context.Database.EnsureCreated();
                var rootCompanies = context.AddCompanyAndChildrenInDatabase();
                //                   -- West Coast --|--- San Fran ---
                var sanFran = rootCompanies.First().Children.Single().Children.First();

                //ATTEMPT
                RetailOutlet.AddTenantToDatabaseWithSaveChanges("New shop", sanFran, context);

                //VERIFY
                var display = context.Tenants.IgnoreQueryFilters().Select(x => x.ToString()).ToList();
                foreach (var line in display)
                {
                    _output.WriteLine($"\"{line}\",");
                }
                display.ShouldEqual(new List<string>
                {
                    "Company: Name = 4U Inc., DataKey = 1|",
                    "SubGroup: Name = West Coast, DataKey = 1|2|",
                    "SubGroup: Name = San Fran, DataKey = 1|2|3|",
                    "RetailOutlet: Name = SF Dress4U, DataKey = 1|2|3|4*",
                    "RetailOutlet: Name = SF Tie4U, DataKey = 1|2|3|5*",
                    "RetailOutlet: Name = SF Shirt4U, DataKey = 1|2|3|6*",
                    "SubGroup: Name = LA, DataKey = 1|2|7|",
                    "RetailOutlet: Name = LA Dress4U, DataKey = 1|2|7|8*",
                    "RetailOutlet: Name = LA Tie4U, DataKey = 1|2|7|9*",
                    "RetailOutlet: Name = LA Shirt4U, DataKey = 1|2|7|a*",
                    "RetailOutlet: Name = New shop, DataKey = 1|2|3|b*",
                });
            }
        }

        [Fact]
        public void TestAddTenantSubGroupOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("accessKey")))
            {
                context.Database.EnsureCreated();
                var rootCompanies = context.AddCompanyAndChildrenInDatabase();
                //                          - West Coast --
                var westCoast = rootCompanies.First().Children.Single();

                //ATTEMPT
                SubGroup.AddTenantToDatabaseWithSaveChanges("Seattle", westCoast, context);

                //VERIFY
                var display = context.Tenants.IgnoreQueryFilters().Select(x => x.ToString()).ToList();
                foreach (var line in display)
                {
                    _output.WriteLine($"\"{line}\",");
                }
                display.ShouldEqual(new List<string>
                {
                    "Company: Name = 4U Inc., DataKey = 1|",
                    "SubGroup: Name = West Coast, DataKey = 1|2|",
                    "SubGroup: Name = San Fran, DataKey = 1|2|3|",
                    "RetailOutlet: Name = SF Dress4U, DataKey = 1|2|3|4*",
                    "RetailOutlet: Name = SF Tie4U, DataKey = 1|2|3|5*",
                    "RetailOutlet: Name = SF Shirt4U, DataKey = 1|2|3|6*",
                    "SubGroup: Name = LA, DataKey = 1|2|7|",
                    "RetailOutlet: Name = LA Dress4U, DataKey = 1|2|7|8*",
                    "RetailOutlet: Name = LA Tie4U, DataKey = 1|2|7|9*",
                    "RetailOutlet: Name = LA Shirt4U, DataKey = 1|2|7|a*",
                    "SubGroup: Name = Seattle, DataKey = 1|2|b|",
                });
            }
        }


    }
}