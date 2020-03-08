// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PermissionParts;
using RefreshClaimsParts;
using ServiceLayer.SeedDemo.Internal;
using ServiceLayer.UserServices.Internal;

namespace ServiceLayer.SeedDemo
{
    /// <summary>
    /// This extension adds the demo data to the database. It will work with "real" databases,
    /// i.e. it will only add data if it isn't already in the database
    /// ONLY USED FOR DEMO
    /// </summary>
    public static class SeedExtensions
    {
        private const string SeedDataDir = "SeedData";
        private const string CompanyDataFilename = "Companies.txt";
        private const string ShopStockFilename = "ShopStock.txt";
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

                var pathUserJson = Path.GetFullPath(Path.Combine(env.WebRootPath, SeedDataDir, UsersFilename));
                var userJson = File.ReadAllText(pathUserJson);
                var userSetup = new DemoUsersSetup(services);
                await userSetup.CheckAddDemoUsersAsync(userJson);
                services.CheckCacheSeedSet();
            }
        }

        private static void CheckCacheSeedSet(this IServiceProvider services)
        {
            var context = services.GetRequiredService<CompanyDbContext>();
            if (context.TimeStores.SingleOrDefault(x => x.Key == AuthChangesConsts.FeatureCacheKey) == null)
            {
                //We seed the TimeStore database with a low number
                context.TimeStores.Add(new TimeStore
                    {Key = AuthChangesConsts.FeatureCacheKey, LastUpdatedTicks = 0});
                context.SaveChanges();
            }
        }

        private static void CheckAddCompanies(IHostingEnvironment env, IServiceProvider services)
        {
            var context = services.GetRequiredService<CompanyDbContext>();
            if (!context.Tenants.IgnoreQueryFilters().Any())
            {
                //No companies so add them and the shop data 
                var pathCompanyData = Path.GetFullPath(Path.Combine(env.WebRootPath, SeedDataDir, CompanyDataFilename));
                var companyData = File.ReadAllLines(pathCompanyData);
                context.AddCompanyAndChildrenInDatabase(companyData);

                var pathShopData = Path.GetFullPath(Path.Combine(env.WebRootPath, SeedDataDir, ShopStockFilename));
                var stockData = File.ReadAllLines(pathShopData);
                context.AddStockToShops(stockData);
                context.SaveChanges();
            }
        }

        private static void CheckAddRoles(IHostingEnvironment env, IServiceProvider services)
        {
            var pathRolesData = Path.GetFullPath(Path.Combine(env.WebRootPath, SeedDataDir, RolesFilename));
            var context = services.GetRequiredService<ExtraAuthorizeDbContext>();
            
            var extraService = new ExtraAuthUsersSetup(context);
            var lines = File.ReadAllLines(pathRolesData);
            foreach (var line in lines)
            {
                var colonIndex = line.IndexOf(':');
                var roleName = line.Substring(0, colonIndex);
                var permissions = line.Substring(colonIndex + 1).Split(',')
                    .Select(x => Enum.Parse(typeof(Permissions), x.Trim(), true))
                    .Cast<Permissions>().ToList();
                extraService.AddUpdateRoleToPermissions(roleName, roleName, permissions);
            }

            context.SaveChanges();
        }
    }
}