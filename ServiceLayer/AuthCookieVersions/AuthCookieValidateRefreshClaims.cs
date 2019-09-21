// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CommonCache;
using DataAuthorize;
using DataLayer.EfCode;
using FeatureAuthorize;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.CodeCalledInStartup;

namespace ServiceLayer.AuthCookieVersions
{
    /// <summary>
    /// This version provides:
    /// - Adds Permissions to the user's claims.
    /// - Adds DataKey to the user's claims
    /// - AND the user's claims are updated if there is a change in the roles/datakey information
    /// </summary>
    public class AuthCookieValidateRefreshClaims : IAuthCookieValidate
    {
        public async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            var authChanges = new AuthChanges();
            var extraContext = context.HttpContext.RequestServices.GetRequiredService<ExtraAuthorizeDbContext>();
            //now we set up the lazy values - I used Lazy for performance reasons, as 99.9% of the time the lazy parts aren't needed
            // ReSharper disable once AccessToDisposedClosure
            var rtoPLazy = new Lazy<CalcAllowedPermissions>(() => new CalcAllowedPermissions(extraContext));
            // ReSharper disable once AccessToDisposedClosure
            var dataKeyLazy = new Lazy<CalcDataKey>(() => new CalcDataKey(extraContext));

            var newClaims = new List<Claim>();
            var originalClaims = context.Principal.Claims.ToList();
            if (originalClaims.All(x => x.Type != PermissionConstants.PackedPermissionClaimType) ||
                authChanges.IsOutOfDateOrMissing(AuthChangesConsts.FeatureCacheKey,
                    originalClaims.SingleOrDefault(x => x.Type == PermissionConstants.LastPermissionsUpdatedClaimType)?.Value,
                    extraContext))
            {
                //Handle the feature permissions
                var userId = originalClaims.GetUserIdFromClaims();
                newClaims.AddRange(await BuildFeatureClaimsAsync(userId, rtoPLazy.Value));
                newClaims.AddRange(BuildDataClaims(userId, dataKeyLazy.Value));

                //Something has changed so we replace the current ClaimsPrincipal with a new one

                newClaims.AddRange(RemoveUpdatedClaimsFromOriginalClaims(originalClaims, newClaims)); //Copy over unchanged claims
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