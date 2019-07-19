using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.UserServices;

namespace PermissionAccessControl2.Controllers
{
    public class CacheController : Controller
    {
        private readonly ICacheRoleService _cacheRoleService;

        public CacheController(ICacheRoleService cacheRoleService)
        {
            _cacheRoleService = cacheRoleService;
        }

        public IActionResult Index()
        {
            return View(_cacheRoleService.ShowExistingCachePermissions(HttpContext.User.Claims));
        }
    }
}