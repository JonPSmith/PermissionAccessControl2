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
using RefreshClaimsParts;
using UserImpersonation.Concrete;

namespace AuthorizeSetup
{
    /// <summary>
    /// This version provides:
    /// - Adds Permissions to the user's claims.
    /// - Adds DataKey to the user's claims
    /// - AND allows for user impersonation
    /// - AND the user's claims are updated if there is a change in the roles/datakey information
    /// </summary>
    public class AuthCookieValidateEverything : IAuthCookieValidate
    {
        /// <summary>
        /// This will set up the user's feature permissions if either of the following states are found
        /// - The current claims doesn't have the PackedPermissionClaimType. This happens when someone logs in.
        /// - If the LastPermissionsUpdatedClaimType is missing (null) or is a lower number that is stored in the TimeStore cache.
        /// It will also add a HierarchicalKeyClaimName claim with the user's data key if not present.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            var extraContext = context.HttpContext.RequestServices.GetRequiredService<ExtraAuthorizeDbContext>();
            var protectionProvider = context.HttpContext.RequestServices.GetService<IDataProtectionProvider>();
            var authChanges = new AuthChanges();

            var originalClaims = context.Principal.Claims.ToList();
            var impHandler = new ImpersonationHandler(context.HttpContext, protectionProvider, originalClaims);
            
            var newClaims = new List<Claim>();
            if (originalClaims.All(x => x.Type != PermissionConstants.PackedPermissionClaimType) ||
                impHandler.ImpersonationChange ||
                authChanges.IsOutOfDateOrMissing(AuthChangesConsts.FeatureCacheKey, 
                    originalClaims.SingleOrDefault(x => x.Type == PermissionConstants.LastPermissionsUpdatedClaimType)?.Value,
                    extraContext))
            {
                var rtoPCalcer = new CalcAllowedPermissions(extraContext);
                var dataKeyCalc = new CalcDataKey(extraContext);

                //Handle the feature permissions
                var permissionUserId = impHandler.GetUserIdForWorkingOutPermissions();
                newClaims.AddRange(await BuildFeatureClaimsAsync(permissionUserId, rtoPCalcer));

                //Handle the DataKey
                var datakeyUserId = impHandler.GetUserIdForWorkingDataKey();
                newClaims.AddRange(BuildDataClaims(datakeyUserId, dataKeyCalc));

                //Something has changed so we replace the current ClaimsPrincipal with a new one

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