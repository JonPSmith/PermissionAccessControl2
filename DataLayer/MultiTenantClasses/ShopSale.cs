// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations.Schema;
using DataAuthorize;
using DataLayer.EfCode;
using GenericServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

namespace DataLayer.MultiTenantClasses
{
    public class ShopSale : ShopLevelDataKeyBase
    {
        private ShopSale() { } //needed by EF Core

        private ShopSale(int numSoldReturned, string returnReason, int tenantItemId, int shopStockId)
        {
            if (numSoldReturned == 0) throw new ArgumentException("cannot be zero", nameof(numSoldReturned));
            if (numSoldReturned < 0 && returnReason == null) throw new ArgumentException("cannot be null if its a return", nameof(returnReason));
            if (tenantItemId == 0) throw new ArgumentException("cannot be zero", nameof(tenantItemId));
            if (shopStockId == 0) throw new ArgumentException("cannot be zero", nameof(shopStockId));

            NumSoldReturned = numSoldReturned;
            ReturnReason = returnReason;
            TenantItemId = tenantItemId;
            ShopStockId = shopStockId;
        }

        public int ShopSaleId { get; private set; }

        /// <summary>
        /// positive number for sale, negative number for return
        /// </summary>
        public int NumSoldReturned { get; private set; }

        /// <summary>
        /// Will be null if sale
        /// </summary>
        public string ReturnReason { get; private set; }

        //------------------------------------------
        //relationships

        public int TenantItemId { get; private set; }

        [ForeignKey(nameof(TenantItemId))]
        public RetailOutlet Shop { get; private set; }

        public int ShopStockId { get; private set; }

        [ForeignKey(nameof(ShopStockId))]
        public ShopStock StockItem { get; private set; }

        //----------------------------------------------
        //create methods

        /// <summary>
        /// This creates a Sale entry, and also update the ShopStock number in stock
        /// </summary>
        /// <param name="numBought"></param>
        /// <param name="tenantItemId"></param>
        /// <param name="shopStockId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IStatusGeneric<ShopSale> CreateSellAndUpdateStock(int numBought, int tenantItemId, int shopStockId, DbContext context)
        {
            if (numBought < 0) throw new ArgumentException("must be positive", nameof(numBought));
            var status = new StatusGenericHandler<ShopSale>();

            var stock = context.Find<ShopStock>(shopStockId);
            if (stock == null)
            {
                status.AddError("Could not find the stock item you requested.");
                return status;
            }
            stock.NumInStock = stock.NumInStock - numBought;
            if (stock.NumInStock < 0)
            {
                status.AddError("There are not enough items of that product to sell.");
                return status;
            }
            var sale = new ShopSale(numBought, null, tenantItemId, shopStockId);
            return status.SetResult(sale);
        }
    }
}