// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.MultiTenantClasses;
using GenericServices;

namespace ServiceLayer.Shop
{
    public class ListStockDto : ILinkToEntity<ShopStock>
    {
        public int ShopStockId { get; set; }
        public string Name { get; set; }
        public decimal RetailPrice { get; set; }
        public int NumInStock { get; set; }

        public string ShopName { get; set; }
    }
}