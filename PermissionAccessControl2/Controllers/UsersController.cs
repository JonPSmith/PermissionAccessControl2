// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses;
using FeatureAuthorize;
using GenericServices;
using Microsoft.AspNetCore.Mvc;
using PermissionParts;
using ServiceLayer.UserServices;

namespace PermissionAccessControl2.Controllers
{
    public class UsersController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View(HttpContext.User);
        }

        public IActionResult Users([FromServices] IListUsersService service)
        {
            return View(service.ListUserWithRolesAndDataTenant());
        }

        public IActionResult AllRoles([FromServices] ICrudServices<ExtraAuthorizeDbContext> services)
        {
            return View(services.ReadManyNoTracked<RoleToPermissions>().ToList());
        }

        public IActionResult UserPermissions()
        {
            var permissionsClaim = HttpContext.User.Claims.SingleOrDefault(c => c.Type == PermissionConstants.PackedPermissionClaimType);
            var permissions = permissionsClaim?.Value.UnpackPermissionsFromString().ToArray();
            return View(permissions);
        }
    }
}