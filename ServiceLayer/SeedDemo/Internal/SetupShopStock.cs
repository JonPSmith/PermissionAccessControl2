// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using DataLayer.EfCode;
using DataLayer.MultiTenantClasses;
using Microsoft.EntityFrameworkCore;

namespace ServiceLayer.SeedDemo.Internal
{
    public static class SetupShopStock
    {
        public static void AddStockToShops(this CompanyDbContext context, string [] lines)
        {
            foreach (var line in lines)
            {
                var colonIndex = line.IndexOf(':');
                var shopName = line.Substring(0, colonIndex);
                var shopDataKey = context.Tenants.IgnoreQueryFilters().OfType<RetailOutlet>()
                    .SingleOrDefault(x => x.Name == shopName)?.DataKey;
                if (shopDataKey == null)
                    throw new ApplicationException($"Could not find a shop of name '{shopName}'");

                var eachStock = from stockAndPrice in line.Substring(colonIndex + 1).Split(',')
                    let parts = stockAndPrice.Split('|').Select(x => x.Trim()).ToArray()
                    select new {Name = parts[0], Price = decimal.Parse(parts[1])};
                foreach (var stock in eachStock)
                {
                    var newStock = new ShopStock {Name = stock.Name, NumInStock = 5, RetailPrice = stock.Price};
                    newStock.SetHierarchicalDataKey(shopDataKey);
                    context.Add(newStock);
                }
            }
        }
    }
}