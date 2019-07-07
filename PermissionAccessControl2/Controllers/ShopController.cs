using System;
using System.Collections.Generic;
using System.Linq;
using DataLayer.EfCode;
using FeatureAuthorize.PolicyCode;
using GenericServices;
using GenericServices.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using PermissionParts;
using ServiceLayer.Shop;

namespace PermissionAccessControl2.Controllers
{
    public class ShopController : Controller
    {
        [HttpGet]
        [HasPermission(Permissions.SalesSell)]
        public IActionResult Till([FromServices] ICrudServices<CompanyDbContext> service)
        {
            var dto = new SellItemDto();
            dto.SetResetDto(service.ReadManyNoTracked<StockSelectDto>().ToList());
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.SalesSell)]
        public IActionResult Till([FromServices] ICrudServices<CompanyDbContext> service, SellItemDto dto)
        {
            if (!ModelState.IsValid)
            {
                dto.SetResetDto(service.ReadManyNoTracked<StockSelectDto>().ToList());
                return View(dto);
            }

            var result = service.CreateAndSave(dto);
            if (service.IsValid)
                return RedirectToAction("BuySuccess", new { message = service.Message, result.ShopSaleId });

            //Error state
            service.CopyErrorsToModelState(ModelState, dto);
            dto.SetResetDto(service.ReadManyNoTracked<StockSelectDto>().ToList());
            return View(dto);
        }

        public IActionResult BuySuccess([FromServices] ICrudServices<CompanyDbContext> service, string message, int shopSaleId)
        {
            var saleInfo = service.ReadSingle<ListSalesDto>(shopSaleId);
            return View(new Tuple<ListSalesDto, string>(saleInfo, message));
        }

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
            var allSales = service.ReadManyNoTracked<ListSalesDto>().ToList();
            var allTheSameShop = allSales.Any() && allSales.All(x => x.StockItemShopName == allSales.First().StockItemShopName);
            return View(new Tuple<List<ListSalesDto>, bool>(allSales, allTheSameShop));
        }
    }
}