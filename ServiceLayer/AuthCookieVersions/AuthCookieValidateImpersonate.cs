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
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.CodeCalledInStartup;
using ServiceLayer.UserImpersonation.Concrete.Internal;

namespace ServiceLayer.AuthCookieVersions
{
    /// <summary>
    /// This is the code that can calculates the feature and data claims for a user.
    /// It also has the ability to recalculate the logged-in user's permissions when the UserRoles/RoleToPermissions change
    /// NOTE: Because it needs to be set up at the start we cannot use any DI items that are scoped or rely on same-instance singletons
    /// </summary>
    public class AuthCookieValidateImpersonate
    {
        private readonly DbContextOptions<ExtraAuthorizeDbContext> _extraAuthContextOptions;
        private readonly IDataProtectionProvider _protectionProvider;
        private readonly IAuthChanges _authChanges;

        public AuthCookieValidateImpersonate(DbContextOptions<ExtraAuthorizeDbContext> extraAuthContextOptions, 
            IDataProtectionProvider protectionProvider)
        {
            _extraAuthContextOptions = extraAuthContextOptions ?? throw new ArgumentNullException(nameof(extraAuthContextOptions));
            _protectionProvider = protectionProvider; //This can be null, in which case impersonation is turned off
            _authChanges = new AuthChanges();
        }

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
            var extraContext = new ExtraAuthorizeDbContext(_extraAuthContextOptions, _authChanges);
            //now we set up the lazy values - I used Lazy for performance reasons, as 99.9% of the time the lazy parts aren't needed
            // ReSharper disable once AccessToDisposedClosure
            var rtoPLazy = new Lazy<CalcAllowedPermissions>(() => new CalcAllowedPermissions(extraContext));
            // ReSharper disable once AccessToDisposedClosure
            var dataKeyLazy = new Lazy<CalcDataKey>(() => new CalcDataKey(extraContext));

            var originalClaims = context.Principal.Claims.ToList();
            var impHandler = new ImpersonationHandler(context.HttpContext, _protectionProvider, originalClaims);
            
            var newClaims = new List<Claim>();
            if (originalClaims.All(x => x.Type != PermissionConstants.PackedPermissionClaimType) ||
                impHandler.ImpersonationChange ||
                _authChanges.IsOutOfDateOrMissing(AuthChangesConsts.FeatureCacheKey, 
                    originalClaims.SingleOrDefault(x => x.Type == PermissionConstants.LastPermissionsUpdatedClaimType)?.Value,
                    extraContext))
            {
                //Handle the feature permissions
                var userId = impHandler.GetUserIdForWorkingOutPermissions();
                newClaims.AddRange(await BuildFeatureClaimsAsync(userId, rtoPLazy.Value));
            }

            if (originalClaims.All(x => x.Type != DataAuthConstants.HierarchicalKeyClaimName) ||
                impHandler.ImpersonationChange)
            {
                var userId = impHandler.GetUserIdForWorkingDataKey();
                newClaims.AddRange(BuildDataClaims(userId, dataKeyLazy.Value));
            }

            if (newClaims.Any())
            {
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
            extraContext.Dispose(); //be tidy and dispose the context.
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