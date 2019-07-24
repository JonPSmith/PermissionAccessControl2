// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DataLayer.EfCode;
using DataLayer.MultiTenantClasses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ServiceLayer.UserServices.Internal;

[assembly: InternalsVisibleTo("Test")]

namespace ServiceLayer.SeedDemo.Internal
{
    /// <summary>
    /// This is the code that sets up the demo user, with their roles and data keys.
    /// It uses the data from in the wwwroot/SeedData/Users.json file
    /// ONLY USED FOR DEMO
    /// </summary>
    internal class DemoUsersSetup
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ExtraAuthorizeDbContext _extraContext;
        private readonly ExtraAuthUsersSetup _extraService;

        public DemoUsersSetup(IServiceProvider services)
        {
            _userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            _extraContext = services.GetRequiredService<ExtraAuthorizeDbContext>();
            _extraService = new ExtraAuthUsersSetup(_extraContext);
        }

        public async Task CheckAddDemoUsersAsync(string usersJson)
        {
            var allOutlets = _extraContext.Tenants.IgnoreQueryFilters().OfType<RetailOutlet>().ToList();
            foreach (var userSpec in JsonConvert.DeserializeObject<List<UserJson>>(usersJson))
            {
                if (userSpec.LinkedTenant.StartsWith("*"))
                {
                    //We need to form names for outlets
                    foreach (var retailOutlet in allOutlets.Where(x => x.Name.EndsWith(userSpec.LinkedTenant.Substring(1))))
                    {
                        var email = retailOutlet.Name.Replace(' ', '-') + userSpec.Email.Substring(1);
                        await CheckAddUser(email, userSpec.RolesCommaDelimited, retailOutlet);
                    }
                }
                else
                {
                    var foundTenant = _extraContext.Tenants.IgnoreQueryFilters()
                        .SingleOrDefault(x => x.Name == userSpec.LinkedTenant);
                    if (foundTenant == null)
                        throw new ApplicationException($"Could not find a tenant named {userSpec.LinkedTenant}.");
                    await CheckAddUser(userSpec.Email, userSpec.RolesCommaDelimited, foundTenant);
                }
            }

            _extraContext.SaveChanges();
        }

        private async Task CheckAddUser(string email, string rolesCommaDelimited, TenantBase linkedTenant)
        {
            var user = await _userManager.CheckAddNewUserAsync(email, email); //password is their email
            foreach (var roleName in rolesCommaDelimited.Split(',').Select(x => x.Trim()))
            {
                _extraService.CheckAddRoleToUser(user.Id, roleName);
            }
            _extraService.AddUpdateDataAccessHierarchical(user.Id, linkedTenant);
            _extraService.CheckAddModules(user.Id, linkedTenant);
        }


        private class UserJson
        {
            public string Email { get; set; }
            public string RolesCommaDelimited { get; set; }
            public string LinkedTenant { get; set; }
        }

    }
}