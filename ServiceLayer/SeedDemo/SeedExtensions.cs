// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.AppClasses.MultiTenantParts;
using DataLayer.EfCode;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PermissionParts;
using ServiceLayer.SeedDemo.Internal;

namespace ServiceLayer.SeedDemo
{
    public static class SeedExtensions
    {
        private const string SeedDataDir = "SeedData";
        private const string CompanyDataFilename = "Companies.txt";
        private const string RolesFilename = "Roles.txt";
        private const string UsersFilename = "Users.json";

        /// <summary>
        /// This will check if tenants and users need to be added to the database,
        /// i.e. it will only add tenants and users if they are not already in the database
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <returns></returns>
        public static async Task CheckSeedDataAndUserAsync(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var env = services.GetRequiredService<IHostingEnvironment>();

                CheckAddCompanies(env, services);
                CheckAddRoles(env, services);

                var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
                var extraContext = services.GetRequiredService<ExtraAuthorizeDbContext>();
                var appContext = services.GetRequiredService<AppDbContext>();
                var pathUserJson = Path.GetFullPath(Path.Combine(env.WebRootPath, SeedDataDir, UsersFilename));
                var userJson = File.ReadAllText(pathUserJson);
                var userSetup = new DemoUsersSetup(userManager, extraContext, appContext);
                await userSetup.CheckAddDemoUsersAsync(userJson);
            }
        }

        private static void CheckAddCompanies(IHostingEnvironment env, IServiceProvider services)
        {
            var pathCompanyData = Path.GetFullPath(Path.Combine(env.WebRootPath, SeedDataDir, CompanyDataFilename));
            var context = services.GetRequiredService<AppDbContext>();
            if (!context.Tenants.IgnoreQueryFilters().Any())
            {
                //No companies 
                var lines = File.ReadAllLines(pathCompanyData);
                context.AddCompanyAndChildrenInDatabase(lines);
            }
        }

        private static void CheckAddRoles(IHostingEnvironment env, IServiceProvider services)
        {
            var pathRolesData = Path.GetFullPath(Path.Combine(env.WebRootPath, SeedDataDir, RolesFilename));
            var context = services.GetRequiredService<ExtraAuthorizeDbContext>();
            
            var extraService = new SetupExtraAuthUsers(context);
            var lines = File.ReadAllLines(pathRolesData);
            foreach (var line in lines)
            {
                var colonIndex = line.IndexOf(':');
                var roleName = line.Substring(0, colonIndex);
                var permissions = line.Substring(colonIndex + 1).Split(',')
                    .Select(x => Enum.Parse(typeof(Permissions), x.Trim(), true))
                    .Cast<Permissions>().ToList();
                extraService.CheckAddNewRole(roleName, permissions);
            }

            context.SaveChanges();
        }
    }
}