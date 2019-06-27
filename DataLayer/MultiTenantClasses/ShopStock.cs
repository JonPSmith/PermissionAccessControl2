// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataAuthorize;

namespace DataLayer.MultiTenantClasses
{
    /// <summary>
    /// This contains an item stocked in the shop, and how many they have
    /// </summary>
    public class ShopStock : HierarchicalKeyBase
    {
        public int ShopStockId { get; set; }
        public string Name { get; set; }
        public decimal RetailPrice { get; set; }
        public int NumInStock { get; set; }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(RetailPrice)}: {RetailPrice}, {nameof(NumInStock)}: {NumInStock}";
        }
    }
}