// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations.Schema;
using DataAuthorize;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DataLayer.MultiTenantClasses
{
    public class ShopSale : HierarchicalKeyBase
    {
        public int ShopSaleId { get; set; }

        /// <summary>
        /// positive number for sale, negative number for return
        /// </summary>
        public int NumSoldReturned { get; set; }

        /// <summary>
        /// Will be null if sale
        /// </summary>
        public string ReturnReason { get; set; }

        //------------------------------------------
        //relationships

        public int TenantItemId { get; set; }

        [ForeignKey(nameof(TenantItemId))]
        public RetailOutlet Shop { get; set; }

        public int ShopStockId { get; set; }

        [ForeignKey(nameof(ShopStockId))]
        public ShopStock StockItem { get; set; }
    }
}