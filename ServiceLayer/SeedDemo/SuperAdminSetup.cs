// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataLayer.EfCode;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.SeedDemo.Internal;
using Microsoft.Extensions.Configuration;
using PermissionParts;

namespace ServiceLayer.SeedDemo
{
    public static class SuperAdminSetup
    {
        private const string SuperAdminRoleName = "SuperAdmin";

        /// <summary>
        /// This ensures there is a SuperAdmin user in the system.
        /// It gets the SuperAdmin's email and password from the "SuperAdmin" section of the appsettings.json file
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static async Task CheckAddSuperAdminAsync(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

                var config = services.GetRequiredService<IConfiguration>();
                var superSection = config.GetSection("SuperAdmin");
                if (superSection == null)
                    return;

                var userEmail = superSection["Email"];
                var userPassword = superSection["Password"];

                var superUser = await userManager.CheckAddNewUserAsync(userEmail, userPassword);

                using (var context = services.GetRequiredService<ExtraAuthorizeDbContext>())
                {
                    var extraService = new SetupExtraAuthUsers(context);
                    extraService.CheckAddNewRole(SuperAdminRoleName, "SuperAdmin Role", new List<Permissions>{ Permissions.AccessAll});
                    extraService.CheckAddRoleToUser(superUser.Id, SuperAdminRoleName);
                    context.SaveChanges();
                }
            }
        }
    }
}