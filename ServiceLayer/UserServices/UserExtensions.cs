using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using FeatureAuthorize;
using PermissionParts;

namespace ServiceLayer.UserServices
{
    public static class UserExtensions
    {
        /// <summary>
        /// This gets the permissions for the currently logged in user (or null if no claim)
        /// </summary>
        /// <param name="usersClaims"></param>
        /// <returns></returns>
        public static IEnumerable<Permissions> PermissionsFromClaims(this IEnumerable<Claim> usersClaims)
        {
            var permissionsClaim =
                usersClaims?.SingleOrDefault(c => c.Type == PermissionConstants.PackedPermissionClaimType);
            return permissionsClaim?.Value.UnpackPermissionsFromString();
        }
    }
}