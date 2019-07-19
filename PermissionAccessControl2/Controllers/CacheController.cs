using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PermissionAccessControl2.Controllers
{
    public class CacheController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}