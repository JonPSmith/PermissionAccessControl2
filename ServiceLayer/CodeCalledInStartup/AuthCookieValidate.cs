// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using CommonCache;
using DataAuthorize;
using FeatureAuthorize;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace ServiceLayer.CodeCalledInStartup
{
    public class AuthCookieValidate
    {
        /// <summary>
        /// This is the code that can calculates the feature permissions for a user
        /// </summary>
        private readonly CalcAllowedPermissions _rtoPCalcer;
        private readonly CalcDataKey _dataKeyCalcer;
        private readonly IAuthChanges _cache;

        public AuthCookieValidate(CalcAllowedPermissions rtoPCalcer, CalcDataKey dataKeyCalcer, IAuthChanges cache)
        {
            _rtoPCalcer = rtoPCalcer;
            _dataKeyCalcer = dataKeyCalcer;
            _cache = cache;
        }

        public async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            var newClaims = new List<Claim>();
            var originalClaims = context.Principal.Claims.ToList();
            if (originalClaims.All(x => x.Type != PermissionConstants.PackedPermissionClaimType) ||
                _cache.IsLowerThan(AuthChangesConsts.FeatureCacheKey, 
                    originalClaims.SingleOrDefault(x => x.Type == PermissionConstants.LastPermissionsUpdatedClaimType)?.Value, null))
            {
                //Handle the feature permissions
                var userId = originalClaims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                newClaims.AddRange(await BuildFeatureClaimsAsync(userId));
            }

            if (originalClaims.All(x => x.Type != DataAuthConstants.HierarchicalKeyClaimName))
            {
                var userId = originalClaims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
                newClaims.AddRange(BuildDataClaims(userId));
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
            }
        }

        private IEnumerable<Claim> RemoveUpdatedClaimsFromOriginalClaims(List<Claim> originalClaims, List<Claim> newClaims)
        {
            var newClaimTypes = newClaims.Select(x => x.Type);
            return originalClaims.Where(x => !newClaimTypes.Contains(x.Type));
        }

        private async Task<List<Claim>> BuildFeatureClaimsAsync(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(PermissionConstants.PackedPermissionClaimType, await _rtoPCalcer.CalcPermissionsForUser(userId)),
                new Claim(PermissionConstants.LastPermissionsUpdatedClaimType, DateTime.UtcNow.Ticks.ToString())
            };
            return claims;
        }

        private List<Claim> BuildDataClaims(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(DataAuthConstants.HierarchicalKeyClaimName, _dataKeyCalcer.CalcDataKeyForUser(userId))
            };
            return claims;
        }
    }
}