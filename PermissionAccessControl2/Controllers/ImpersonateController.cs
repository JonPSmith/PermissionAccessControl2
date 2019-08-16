// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using FeatureAuthorize.PolicyCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PermissionParts;
using ServiceLayer.UserImpersonation;
using ServiceLayer.UserServices;

namespace PermissionAccessControl2.Controllers
{
    public class ImpersonateController : Controller
    {
        //You would normally protect this with [HasPermission(Permissions.Impersonate)], but I left that off on 
        public IActionResult Index([FromServices] IListUsersService service)
        {
            if (User.InImpersonationMode())
                return RedirectToAction(nameof(Message),
                    new { errorMessage = "You are already in impersonation mode."});
            return View(service.ListUserWithRolesAndDataTenant());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.Impersonate)]
        public IActionResult StartNormal(string userId, string userName, [FromServices] IImpersonationService service)
        {
            var errorMessage = service.StartImpersonation(userId, userName, false);
            return RedirectToAction(nameof(Message), 
                new {errorMessage, successMessage =$"You are now impersonating user {userName} with their permissions." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HasPermission(Permissions.ImpersonateWithOwnPermissions)]
        public IActionResult StartEnhanced(string userId, string userName, [FromServices] IImpersonationService service)
        {
            var errorMessage = service.StartImpersonation(userId, userName, true);
            return RedirectToAction(nameof(Message),
                new { errorMessage, successMessage = $"You are now impersonating user {userName} with your own permissions." });
        }

        [Authorize] //you must be logged in
        //Note: anyone call call Stop, as when impersonating someone you don't know what permissions (if any) that they have
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