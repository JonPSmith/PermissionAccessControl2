// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses;
using DataLayer.MultiTenantClasses;
using Microsoft.AspNetCore.Identity;

namespace ServiceLayer.UserServices.Concrete
{
    public class ListUsersService : IListUsersService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ExtraAuthorizeDbContext _extraContext;

        public ListUsersService(UserManager<IdentityUser> userManager, ExtraAuthorizeDbContext extraContext)
        {
            _userManager = userManager;
            _extraContext = extraContext;
        }

        public List<ListUsersDto> ListUserWithRolesAndDataTenant()
        {
            var result = new List<ListUsersDto>();
            foreach (var user in _userManager.Users)
            {
                var userRoleNames = _extraContext.UserToRoles.Where(x => x.UserId == user.Id).Select(x => x.RoleName);
                var dataEntry = _extraContext.Find<UserDataHierarchical>(user.Id);
                string tenantName = "no linked tenant";
                string companyName = null;
                if (dataEntry != null)
                {
                    var linkedTenant = _extraContext.Find<TenantBase>(dataEntry.LinkedTenantId);
                    tenantName = linkedTenant?.Name ?? "tenant not found";
                    if (linkedTenant != null)
                        companyName = _extraContext.Find<TenantBase>(linkedTenant.ExtractCompanyId())?.Name;
                }

                result.Add(new ListUsersDto(user.Id, user.UserName, string.Join(", ", userRoleNames), companyName, tenantName));
            }

            return result;
        }
    }
}