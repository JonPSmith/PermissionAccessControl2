// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfCode;
using DataLayer.MultiTenantClasses;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.CodeCalledInStartup;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataAuthorizeTests
{
    public class TestShopData
    {
        [Fact]
        public void TestCreateValidDatabaseOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("accessKey")))
            {
                //ATTEMPT
                context.Database.EnsureCreated();

                //VERIFY
            }
        }

        [Fact]
        public void TestQueryFilterWorksOnShopStock()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("accessKey*")))
            {
                context.Database.EnsureCreated();
                context.Add(new ShopStock { Name = "dress" });
                context.SaveChanges();

            }
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("DIFF-accessKey*")))
            {
                //ATTEMPT
                var stocksFiltered = context.ShopStocks.ToList();
                var stocksNotFiltered = context.ShopStocks.IgnoreQueryFilters().ToList();

                //VERIFY
                stocksFiltered.Count.ShouldEqual(0);
                stocksNotFiltered.Count.ShouldEqual(1);
                stocksNotFiltered.First().DataKey.ShouldEqual("accessKey*");
            }
        }

        [Fact]
        public void TestQueryFilterWorksOnShopSale()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("accessKey*")))
            {
                context.Database.EnsureCreated();
                context.Add(new ShopSale{ NumSoldReturned = 1, StockItem = new ShopStock { Name = "dress" }});
                context.SaveChanges();

            }
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("DIFF-accessKey*")))
            {
                //ATTEMPT
                var salesFiltered = context.ShopSales.ToList();
                var salesNotFiltered = context.ShopSales.IgnoreQueryFilters().ToList();

                //VERIFY
                salesFiltered.Count.ShouldEqual(0);
                salesNotFiltered.Count.ShouldEqual(1);
                salesNotFiltered.First().DataKey.ShouldEqual("accessKey*");
            }
        }

        [Fact]
        public void TestDataKeyNotSetIfProvidedKeyIsNull()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider(null)))
            {
                context.Database.EnsureCreated();
                var stock = new ShopStock {Name = "dress"};
                stock.SetHierarchicalDataKey("accessKey*");
                context.Add(stock);
                context.SaveChanges();

            }
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("accessKey*")))
            {
                //ATTEMPT
                var stocksFiltered = context.ShopStocks.ToList();

                //VERIFY
                stocksFiltered.Count.ShouldEqual(1);
            }
        }
    }
}