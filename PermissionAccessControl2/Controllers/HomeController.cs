using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DataKeyParts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PermissionAccessControl2.Models;
using ServiceLayer.UserServices;
using ServiceLayer.UserServices.Concrete;

namespace PermissionAccessControl2.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index([FromServices] IOptions<DemoSetupOptions> demoSetup)
        {
            return View(demoSetup.Value);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
