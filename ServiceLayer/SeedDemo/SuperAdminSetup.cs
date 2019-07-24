// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.SeedDemo.Internal;
using Microsoft.Extensions.Configuration;
using PermissionParts;
using ServiceLayer.UserServices.Internal;

namespace ServiceLayer.SeedDemo
{
    public static class SuperAdminSetup
    {
        private const string SuperAdminRoleName = "SuperAdmin";

        /// <summary>
        /// This ensures there is a SuperAdmin user in the system.
        /// It gets the SuperAdmin's email and password from the "SuperAdmin" section of the appsettings.json file
        /// NOTE: for security reasons I only allows one user with the RoleName of <see cref="SuperAdminRoleName"/> 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static async Task CheckAddSuperAdminAsync(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var extraContext = services.GetRequiredService<ExtraAuthorizeDbContext>();
                if (extraContext.UserToRoles.Any(x => x.RoleName == SuperAdminRoleName))
                    //For security reasons there can only be one user with the SuperAdminRoleName
                    return;

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
                    var extraService = new ExtraAuthUsersSetup(context);
                    extraService.AddUpdateRoleToPermissions(SuperAdminRoleName, "SuperAdmin Role", new List<Permissions>{ Permissions.AccessAll});
                    extraService.CheckAddRoleToUser(superUser.Id, SuperAdminRoleName);
                    context.SaveChanges();
                }
            }
        }
    }
}