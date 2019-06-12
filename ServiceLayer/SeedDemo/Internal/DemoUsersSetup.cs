// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DataLayer.AppClasses.MultiTenantParts;
using DataLayer.EfCode;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("Test")]

namespace ServiceLayer.SeedDemo.Internal
{
    internal class DemoUsersSetup
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _appContext;
        private readonly ExtraAuthorizeDbContext _extraContext;
        private readonly SetupExtraAuthUsers _extraService;

        public DemoUsersSetup(UserManager<IdentityUser> userManager, ExtraAuthorizeDbContext extraContext, AppDbContext appContext)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _extraContext = extraContext ?? throw new ArgumentNullException(nameof(extraContext));
            _extraService = new SetupExtraAuthUsers(extraContext);
            _appContext = appContext;
        }

        public async Task CheckAddDemoUsersAsync(string usersJson)
        {
            var allOutlets = _appContext.Tenants.IgnoreQueryFilters().OfType<RetailOutlet>().ToList();
            foreach (var userSpec in JsonConvert.DeserializeObject<List<UserJson>>(usersJson))
            {
                if (userSpec.LinkedTenant.StartsWith("*"))
                {
                    //We need to form names for outlets
                    foreach (var retailOutlet in allOutlets.Where(x => x.Name.EndsWith(userSpec.LinkedTenant.Substring(1))))
                    {
                        var email = retailOutlet.Name.Replace(" ", "") + userSpec.Email.Substring(1);
                        await CheckAddUser(email, userSpec.RolesCommaDelimited, retailOutlet);
                    }
                }
                else
                {
                    var foundTenant = _appContext.Tenants.IgnoreQueryFilters()
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
            _extraService.CheckAddDataAccessHierarchical(user.Id, linkedTenant);
        }


        private class UserJson
        {
            public string Email { get; set; }
            public string RolesCommaDelimited { get; set; }
            public string LinkedTenant { get; set; }
        }

    }
}