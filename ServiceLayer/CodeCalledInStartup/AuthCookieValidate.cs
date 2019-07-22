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
using Microsoft.EntityFrameworkCore;

namespace ServiceLayer.CodeCalledInStartup
{
    /// <summary>
    /// This is the code that can calculates the feature and data claims for a user.
    /// Because it needs to be set up at the start we cannot use any DI items that are scoped.
    /// </summary>
    public class AuthCookieValidate
    {
        private readonly DbContextOptions<ExtraAuthorizeDbContext> _extraAuthContextOptions;
        private readonly IAuthChanges _authChanges;

        public AuthCookieValidate(DbContextOptions<ExtraAuthorizeDbContext> extraAuthContextOptions, IAuthChanges authChanges)
        {
            _extraAuthContextOptions = extraAuthContextOptions;
            _authChanges = authChanges;
        }

        public async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            //now we set up the lazy values - I used Lazy for performance reasons, as 99.9% of the time the lazy parts aren't needed
            var contextLazy = new Lazy<ExtraAuthorizeDbContext>(() => new ExtraAuthorizeDbContext(_extraAuthContextOptions, _authChanges));
            var rtoPLazy = new Lazy<CalcAllowedPermissions>(() => new CalcAllowedPermissions(contextLazy.Value));
            var dataKeyLazy = new Lazy<CalcDataKey>(() => new CalcDataKey(contextLazy.Value));

            var newClaims = new List<Claim>();
            var originalClaims = context.Principal.Claims.ToList();
            if (originalClaims.All(x => x.Type != PermissionConstants.PackedPermissionClaimType) ||
                _authChanges.IsOutOfDateOrMissing(AuthChangesConsts.FeatureCacheKey, 
                    originalClaims.SingleOrDefault(x => x.Type == PermissionConstants.LastPermissionsUpdatedClaimType)?.Value,
                    // ReSharper disable once AccessToDisposedClosure
                    () => contextLazy.Value))
            {
                //Handle the feature permissions
                var userId = originalClaims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                newClaims.AddRange(await BuildFeatureClaimsAsync(userId, rtoPLazy.Value));
            }

            if (originalClaims.All(x => x.Type != DataAuthConstants.HierarchicalKeyClaimName))
            {
                var userId = originalClaims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                newClaims.AddRange(BuildDataClaims(userId, dataKeyLazy.Value));
            }

            if (newClaims.Any())
            {
                //Something has changed so we replace the current ClaimsPrincipal with a new one

                newClaims.AddRange(RemoveUpdatedClaimsFromOriginalClaims(originalClaims, newClaims)); //Copy over unchanged claims
                //Build a new ClaimsPrincipal and use it to replace the current ClaimsPrincipal
                var identity = new ClaimsIdentity(newClaims, "Cookie");
                var newPrincipal = new ClaimsPrincipal(identity);
                context.ReplacePrincipal(newPrincipal);
                //THIS IS IMPORTANT: This updates the cookie, otherwise this calc will be done every HTTP request
                context.ShouldRenew = true;

                contextLazy.Value.Dispose();
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