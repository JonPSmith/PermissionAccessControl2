// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using DataLayer.MultiTenantClasses;
using GenericServices;

namespace ServiceLayer.Shop
{
    public class SellItemDto : ILinkToEntity<ShopSale>
    {
        public List<StockSelectDto> DropdownData { get; set; }

        public int ShopStockId { get; set; }

        public int NumBought { get; set; } = 1;

        public int TenantItemId { get; set; }

        /// <summary>
        /// This holds the PK of the created ShopSale
        /// </summary>
        public int ShopSaleId { get; set; }

        public void SetResetDto(List<StockSelectDto> stockList)
        {
            DropdownData = stockList;
            TenantItemId = stockList.FirstOrDefault()?.TenantItemId ?? 0;
        }
    }
}