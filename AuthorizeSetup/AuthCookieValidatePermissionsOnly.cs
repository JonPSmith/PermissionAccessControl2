// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataLayer.EfCode;
using FeatureAuthorize;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;

namespace AuthorizeSetup
{
    /// <summary>
    /// This is the very basic version which only provides:
    /// - Adds Permissions to the user's claims.
    /// - NO DataKey
    /// </summary>
    public class AuthCookieValidatePermissionsOnly : IAuthCookieValidate
    {
        public async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            if (context.Principal.Claims.Any(x => x.Type == PermissionConstants.PackedPermissionClaimType))
                return;

            //No permissions in the claims, so we need to add it. This is only happen once after the user has logged in
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ExtraAuthorizeDbContext>();
            var rtoPCalcer = new CalcAllowedPermissions(dbContext);

            var claims = new List<Claim>();
            claims.AddRange(context.Principal.Claims); //Copy over existing claims
            //Now calculate the Permissions Claim value and add it
            claims.Add(new Claim(PermissionConstants.PackedPermissionClaimType,
                await rtoPCalcer.CalcPermissionsForUserAsync(context.Principal.Claims.GetUserIdFromClaims())));

            //Build a new ClaimsPrincipal and use it to replace the current ClaimsPrincipal
            var identity = new ClaimsIdentity(claims, "Cookie");
            var newPrincipal = new ClaimsPrincipal(identity);
            context.ReplacePrincipal(newPrincipal);
            //THIS IS IMPORTANT: This updates the cookie, otherwise this calc will be done every HTTP request
            context.ShouldRenew = true;  
        }
    }
}