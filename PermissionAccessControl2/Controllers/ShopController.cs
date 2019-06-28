using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer.EfCode;
using FeatureAuthorize.PolicyCode;
using GenericServices;
using Microsoft.AspNetCore.Mvc;
using PermissionParts;
using ServiceLayer.Shop;

namespace PermissionAccessControl2.Controllers
{
    public class ShopController : Controller
    {


        [HasPermission(Permissions.StockRead)]
        public IActionResult Stock([FromServices] ICrudServices<CompanyDbContext> service)
        {
            var allStock = service.ReadManyNoTracked<ListStockDto>().ToList();
            var allTheSameShop = allStock.Any() && allStock.All(x => x.ShopName == allStock.First().ShopName);
            return View(new Tuple<List<ListStockDto>, bool>(allStock, allTheSameShop));
        }

        [HasPermission(Permissions.SalesRead)]
        public IActionResult Sales([FromServices] ICrudServices<CompanyDbContext> service)
        {
            var allStock = service.ReadManyNoTracked<ListSalesDto>().ToList();
            var allTheSameShop = allStock.Any() && allStock.All(x => x.ShopName == allStock.First().ShopName);
            return View(new Tuple<List<ListSalesDto>, bool>(allStock, allTheSameShop));
        }
    }
}