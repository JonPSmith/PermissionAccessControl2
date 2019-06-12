// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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

        public AuthCookieValidate(CalcAllowedPermissions rtoPCalcer)
        {
            _rtoPCalcer = rtoPCalcer;
        }


        public async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            var newClaims = new List<Claim>();
            if (context.Principal.Claims.All(x => x.Type != PermissionConstants.PackedPermissionClaimType))
            {
                //Handle the feature permissions
                var userId = context.Principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
                newClaims.AddRange(await BuildFeatureClaimsAsync(userId));
            }

            if (context.Principal.Claims.All(x => x.Type != DataAuthConstants.HierarchicalKeyClaimName))
            {
                var userId = context.Principal.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
                newClaims.AddRange(await BuildDataClaimsAsync(userId));
            }

            if (newClaims.Any())
            {
                //Something has changed so we replace the current ClaimsPrincipal with a new one

                newClaims.AddRange(context.Principal.Claims); //Copy over existing claims
                //Build a new ClaimsPrincipal and use it to replace the current ClaimsPrincipal
                var identity = new ClaimsIdentity(newClaims, "Cookie");
                var newPrincipal = new ClaimsPrincipal(identity);
                context.ReplacePrincipal(newPrincipal);
                //THIS IS IMPORTANT: This updates the cookie, otherwise this calc will be done every HTTP request
                context.ShouldRenew = true;
            }
        }

        private async Task<List<Claim>> BuildFeatureClaimsAsync(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(PermissionConstants.PackedPermissionClaimType, await _rtoPCalcer.CalcPermissionsForUser(userId))
            };
            return claims;
        }

        private async Task<List<Claim>> BuildDataClaimsAsync(string userId)
        {
            var claims = new List<Claim>
            {
                new Claim(DataAuthConstants.HierarchicalKeyClaimName,await _rtoPCalcer.CalcPermissionsForUser(userId))
            };
            return claims;
        }
    }
}