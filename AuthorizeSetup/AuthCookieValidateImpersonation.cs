// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAuthorize;
using DataKeyParts;
using DataLayer.EfCode;
using FeatureAuthorize;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using UserImpersonation.Concrete;

namespace AuthorizeSetup
{
    /// <summary>
    /// This version provides:
    /// - Adds Permissions to the user's claims.
    /// - Adds DataKey to the user's claims
    /// - AND user impersonation
    /// </summary>
    public class AuthCookieValidateImpersonation : IAuthCookieValidate
    {
        public async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            var originalClaims = context.Principal.Claims.ToList();
            var protectionProvider = context.HttpContext.RequestServices.GetService<IDataProtectionProvider>();
            var impHandler = new ImpersonationHandler(context.HttpContext, protectionProvider, originalClaims);
            
            var newClaims = new List<Claim>();
            if (originalClaims.All(x => x.Type != PermissionConstants.PackedPermissionClaimType) ||
                impHandler.ImpersonationChange)
            {
                //There is no PackedPermissionClaimType or there was a change in the impersonation state
                
                var extraContext = context.HttpContext.RequestServices.GetRequiredService<ExtraAuthorizeDbContext>();
                var rtoPCalcer = new CalcAllowedPermissions(extraContext);
                var dataKeyCalc = new CalcDataKey(extraContext);

                var userId = impHandler.GetUserIdForWorkingDataKey();
                newClaims.AddRange(await BuildFeatureClaimsAsync(userId, rtoPCalcer));
                newClaims.AddRange(BuildDataClaims(userId, dataKeyCalc));

                newClaims.AddRange(RemoveUpdatedClaimsFromOriginalClaims(originalClaims, newClaims)); //Copy over unchanged claims
                impHandler.AddOrRemoveImpersonationClaim(newClaims);
                //Build a new ClaimsPrincipal and use it to replace the current ClaimsPrincipal
                var identity = new ClaimsIdentity(newClaims, "Cookie");
                var newPrincipal = new ClaimsPrincipal(identity);
                context.ReplacePrincipal(newPrincipal);
                //THIS IS IMPORTANT: This updates the cookie, otherwise this calc will be done every HTTP request
                context.ShouldRenew = true;             
            }
        }

        private IEnumerable<Claim> RemoveUpdatedClaimsFromOriginalClaims(List<Claim> originalClaims, List<Claim> newClaims)
        {
            var newClaimTypes = newClaims.Select(x => x.Type);
            return originalClaims.Where(x => !newClaimTypes.Contains(x.Type));
        }

        private async Task<List<Claim>> BuildFeatureClaimsAsync(string userId, CalcAllowedPermissions rtoP)
        {
            var claims = new List<Claim>
            {
                new Claim(PermissionConstants.PackedPermissionClaimType, await rtoP.CalcPermissionsForUserAsync(userId)),
                new Claim(PermissionConstants.LastPermissionsUpdatedClaimType, DateTime.UtcNow.Ticks.ToString())
            };
            return claims;
        }

        private List<Claim> BuildDataClaims(string userId, CalcDataKey dataKeyCalc)
        {
            var claims = new List<Claim>
            {
                new Claim(DataAuthConstants.HierarchicalKeyClaimName, dataKeyCalc.CalcDataKeyForUser(userId))
            };
            return claims;
        }
    }
}