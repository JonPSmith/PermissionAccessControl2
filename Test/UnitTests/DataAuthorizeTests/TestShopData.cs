// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfCode;
using DataLayer.MultiTenantClasses;
using Microsoft.EntityFrameworkCore;
using PermissionParts;
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
                var company = Company.AddTenantToDatabaseWithSaveChanges("TestCompany", PaidForModules.None, context);
                var shop = RetailOutlet.AddTenantToDatabaseWithSaveChanges("TestShop", company, context);
                context.Add(new ShopStock { Name = "dress", Shop = shop});
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
        public void TestShopSaleCreatedProperly()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("accessKey*")))
            {
                context.Database.EnsureCreated();
                var company = Company.AddTenantToDatabaseWithSaveChanges("TestCompany", PaidForModules.None, context);
                var shop = RetailOutlet.AddTenantToDatabaseWithSaveChanges("TestShop", company, context);
                var shopStock = new ShopStock { Name = "dress", RetailPrice = 12, NumInStock = 2, Shop = shop };
                context.Add(shopStock);
                context.SaveChanges();

                //ATTEMPT
                var status = ShopSale.CreateSellAndUpdateStock(1, shopStock.ShopStockId, context);
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                context.Add(status.Result);
                context.SaveChanges();

                //VERIFY
                context.ShopSales.Count().ShouldEqual(1);
                context.ShopStocks.First().NumInStock.ShouldEqual(1);
            }
        }

        [Fact]
        public void TestShopSaleReadProperly()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<CompanyDbContext>();
            using (var context = new CompanyDbContext(options, new FakeGetClaimsProvider("accessKey*")))
            {
                context.Database.EnsureCreated();
                var company = Company.AddTenantToDatabaseWithSaveChanges("TestCompany", PaidForModules.None, context);
                var shop = RetailOutlet.AddTenantToDatabaseWithSaveChanges("TestShop", company, context);
                var shopStock = new ShopStock { Name = "dress", RetailPrice = 12, NumInStock = 2, Shop = shop };
                context.Add(shopStock);
                context.SaveChanges();

                //ATTEMPT
                var status = ShopSale.CreateSellAndUpdateStock(1, shopStock.ShopStockId, context);
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                context.Add(status.Result);
                context.SaveChanges();

                //VERIFY
                var salesNotFiltered = context.ShopSales.IgnoreQueryFilters()
                    .Include(x => x.StockItem).ThenInclude(x => x.Shop)
                    .ToList();

                salesNotFiltered.Count.ShouldEqual(1);
                salesNotFiltered.First().StockItem.ShouldNotBeNull();
                salesNotFiltered.First().StockItem.Shop.ShouldNotBeNull();
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
                var company = Company.AddTenantToDatabaseWithSaveChanges("TestCompany", PaidForModules.None, context);
                var shop = RetailOutlet.AddTenantToDatabaseWithSaveChanges("TestShop", company, context);
                var shopStock = new ShopStock { Name = "dress", RetailPrice = 12, NumInStock = 2, Shop = shop };
                context.Add(shopStock);
                context.SaveChanges();
                var status = ShopSale.CreateSellAndUpdateStock(1, shopStock.ShopStockId, context);
                status.IsValid.ShouldBeTrue(status.GetAllErrors());
                context.Add(status.Result);
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
                var company = Company.AddTenantToDatabaseWithSaveChanges("TestCompany", PaidForModules.None, context);
                var shop = RetailOutlet.AddTenantToDatabaseWithSaveChanges("TestShop", company, context);
                var stock = new ShopStock {Name = "dress", Shop = shop};
                stock.SetShopLevelDataKey("accessKey*");
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