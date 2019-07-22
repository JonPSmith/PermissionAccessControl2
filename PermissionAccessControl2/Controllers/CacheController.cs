using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
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

        public IActionResult Toggle()
        {
            _cacheRoleService.ToggleCacheRole();
            return RedirectToAction("Index");
        }

        public IActionResult ShowUpdateTime()
        {
            return View(_cacheRoleService.GetFeatureLastUpdated());
        }
    }
}