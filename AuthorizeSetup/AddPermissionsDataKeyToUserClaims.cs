// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;
using DataAuthorize;
using DataKeyParts;
using DataLayer.EfCode;
using FeatureAuthorize;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace AuthorizeSetup
{
    /// <summary>
    /// This version provides:
    /// - Adds Permissions to the user's claims.
    /// - Adds DataKey to the user's claims
    /// </summary>
    // Thanks to https://korzh.com/blogs/net-tricks/aspnet-identity-store-user-data-in-claims
    public class AddPermissionsDataKeyToUserClaims : UserClaimsPrincipalFactory<IdentityUser>
    {
        private readonly ExtraAuthorizeDbContext _extraAuthDbContext;

        public AddPermissionsDataKeyToUserClaims(UserManager<IdentityUser> userManager, IOptions<IdentityOptions> optionsAccessor,
            ExtraAuthorizeDbContext extraAuthDbContext)
            : base(userManager, optionsAccessor)
        {
            _extraAuthDbContext = extraAuthDbContext;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            var userId = identity.Claims.GetUserIdFromClaims();
            var rtoPCalcer = new CalcAllowedPermissions(_extraAuthDbContext);
            identity.AddClaim(new Claim(PermissionConstants.PackedPermissionClaimType, await rtoPCalcer.CalcPermissionsForUserAsync(userId)));
            var dataKeyCalcer = new CalcDataKey(_extraAuthDbContext);
            identity.AddClaim(new Claim(DataAuthConstants.HierarchicalKeyClaimName, dataKeyCalcer.CalcDataKeyForUser(userId)));
            return identity;
        }
    }

}