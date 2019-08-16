// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.UserImpersonation;
using ServiceLayer.UserServices;

namespace PermissionAccessControl2.Controllers
{
    public class ImpersonateController : Controller
    {
        // GET
        public IActionResult Index([FromServices] IListUsersService service)
        {
            if (User.InImpersonationMode())
                return RedirectToAction(nameof(Message),
                    new { errorMessage = "You are already in impersonation mode."});
            return View(service.ListUserWithRolesAndDataTenant());
        }

        [HttpPost]
        public IActionResult Start(string userId, string userName, bool keepOwnPermissions,
            [FromServices] IImpersonationService service)
        {
            var errorMessage = service.StartImpersonation(userId, userName, keepOwnPermissions);
            return RedirectToAction(nameof(Message), 
                new {errorMessage, successMessage =$"You are now impersonating user {userName}." });
        }

        public IActionResult Stop([FromServices] IImpersonationService service)
        {
            var errorMessage = service.StopImpersonation();
            return RedirectToAction(nameof(Message),
                new { errorMessage, successMessage = $"You have stopped impersonating another user." });
        }

        public IActionResult Message(string errorMessage, string successMessage)
        {
            return View(new Tuple<string, string>(errorMessage, successMessage));
        }


    }
}