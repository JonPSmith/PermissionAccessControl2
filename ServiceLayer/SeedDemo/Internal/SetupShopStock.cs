// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.EfCode;
using DataLayer.MultiTenantClasses;
using Microsoft.EntityFrameworkCore;

namespace ServiceLayer.SeedDemo.Internal
{
    /// <summary>
    /// This adds stock to the shops using the data given by the wwwroot/SeedData/ShopStock.txt file
    /// ONLY USED FOR DEMO
    /// </summary>
    public static class SetupShopStock
    {
        public static void AddStockToShops(this CompanyDbContext context, string [] lines)
        {
            foreach (var line in lines)
            {
                var colonIndex = line.IndexOf(':');
                var shopName = line.Substring(0, colonIndex);
                var shop = context.Tenants.IgnoreQueryFilters().OfType<RetailOutlet>()
                    .SingleOrDefault(x => x.Name == shopName);
                if (shop == null)
                    throw new ApplicationException($"Could not find a shop of name '{shopName}'");

                var eachStock = from stockAndPrice in line.Substring(colonIndex + 1).Split(',')
                    let parts = stockAndPrice.Split('|').Select(x => x.Trim()).ToArray()
                    select new {Name = parts[0], Price = decimal.Parse(parts[1])};
                foreach (var stock in eachStock)
                {
                    var newStock = new ShopStock {Name = stock.Name, NumInStock = 5, RetailPrice = stock.Price, Shop = shop};
                    newStock.SetShopLevelDataKey(shop.DataKey);
                    context.Add(newStock);
                }
            }
        }
    }
}