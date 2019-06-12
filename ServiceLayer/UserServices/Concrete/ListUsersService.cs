// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using DataLayer.AppClasses.MultiTenantParts;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses;
using Microsoft.AspNetCore.Identity;

namespace ServiceLayer.UserServices.Concrete
{
    public class ListUsersService
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
                var tenantName = dataEntry != null
                    ? _extraContext.Find<TenantBase>(dataEntry.LinkedTenantId)?.Name ?? "tenant not found"
                    : "no linked tenant";
                result.Add(new ListUsersDto(user.UserName, string.Join(", ", userRoleNames), tenantName));
            }

            return result;
        }
    }
}