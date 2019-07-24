// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfCode;
using DataLayer.MultiTenantClasses;
using GenericServices.PublicButHidden;
using GenericServices.Setup;
using Microsoft.EntityFrameworkCore;
using PermissionParts;
using ServiceLayer.CodeCalledInStartup;
using ServiceLayer.Shop;
using Test.FakesAndMocks;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.ShopServices
{
    public class TestShopServices
    {


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
                var stock = new ShopStock {Name = "dress", NumInStock = 5, Shop = shop};
                context.Add(stock);
                context.SaveChanges();

                var utData = context.SetupSingleDtoAndEntities<SellItemDto>();
                var service = new CrudServices(context, utData.ConfigAndMapper);

                //ATTEMPT
                var dto = new SellItemDto
                {
                    TenantItemId = shop.TenantItemId,
                    ShopStockId = stock.ShopStockId,
                    NumBought = 1
                };
                var shopSale = service.CreateAndSave(dto);

                //VERIFY
                service.IsValid.ShouldBeTrue(service.GetAllErrors());
                context.ShopSales.Count().ShouldEqual(1);
                context.ShopStocks.Single().NumInStock.ShouldEqual(4);
            }
        }

        
    }
}