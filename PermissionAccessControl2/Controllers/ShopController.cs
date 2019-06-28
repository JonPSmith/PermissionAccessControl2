using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer.EfCode;
using GenericServices;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Stock;

namespace PermissionAccessControl2.Controllers
{
    public class ShopController : Controller
    {
        public IActionResult Stock([FromServices] ICrudServices<CompanyDbContext> service)
        {
            var allStock = service.ReadManyNoTracked<ListStockDto>().ToList();
            var allTheSameShop = allStock.Any() && allStock.All(x => x.ShopName == allStock.First().ShopName);
            return View(new Tuple<List<ListStockDto>, bool>(allStock, allTheSameShop));
        }
    }
}