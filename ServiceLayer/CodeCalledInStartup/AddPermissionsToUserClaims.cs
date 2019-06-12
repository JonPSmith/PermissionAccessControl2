// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAuthorize;
using DataLayer.EfCode;
using FeatureAuthorize;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ServiceLayer.CodeCalledInStartup
{
    public class AddPermissionsToUserClaims : UserClaimsPrincipalFactory<IdentityUser>
    {
        private readonly ExtraAuthorizeDbContext _extraAuthDbContext;

        public AddPermissionsToUserClaims(UserManager<IdentityUser> userManager, IOptions<IdentityOptions> optionsAccessor,
            ExtraAuthorizeDbContext extraAuthDbContext)
            : base(userManager, optionsAccessor)
        {
            _extraAuthDbContext = extraAuthDbContext;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
        {
            var identity = await base.GenerateClaimsAsync(user);
            var userId = identity.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var rtoPCalcer = new CalcAllowedPermissions(_extraAuthDbContext);
            identity.AddClaim(new Claim(PermissionConstants.PackedPermissionClaimType,await rtoPCalcer.CalcPermissionsForUser(userId)));
            var dataKeyCalcer = new CalcDataKey(_extraAuthDbContext);
            identity.AddClaim(new Claim(DataAuthConstants.HierarchicalKeyClaimName, dataKeyCalcer.CalcDataKeyForUser(userId)));
            return identity;
        }
    }

}