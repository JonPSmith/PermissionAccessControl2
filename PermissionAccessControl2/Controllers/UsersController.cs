// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc;

namespace PermissionAccessControl2.Controllers
{
    public class UsersController : Controller
    {
        // GET
        public IActionResult Index()
        {
            return View(HttpContext.User);
        }
    }
}