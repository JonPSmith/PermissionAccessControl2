using System.Collections.Generic;
using System.Security.Claims;
using PermissionParts;

namespace ServiceLayer.UserServices
{
    public interface ICacheRoleService
    {
        IEnumerable<Permissions> ShowExistingCachePermissions(IEnumerable<Claim> usersClaims);
        void ToggleCacheRole();
        IEnumerable<string> GetFeatureLastUpdated(IEnumerable<Claim> usersClaims);
    }
}