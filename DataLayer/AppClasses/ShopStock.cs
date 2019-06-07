// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataAuthorize;

namespace DataLayer.AppClasses
{
    public class ShopStock : DataKeyBase, ITenantKey
    {
        public int ShopStockId { get; set; }
        public string Name { get; set; }
        public int NumInStock { get; set; }
    }
}