using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses;
using FeatureAuthorize;
using PermissionParts;
using RefreshClaimsParts;
using ServiceLayer.UserServices.Internal;

namespace ServiceLayer.UserServices.Concrete
{
    /// <summary>
    /// This is used to test that the dynamic update of a logged-in user happens if the UpdateCookieOnChange is true.
    /// It does this by changing the permissions in the Role called <see cref="CacheRoleName"/>,
    /// which always has the permission <see cref="Permissions.Cache1"/>, and can have permission <see cref="Permissions.Cache2"/>
    /// </summary>
    public class CacheRoleService : ICacheRoleService
    {
        //NOTE: If you change this you need to change the Roles.txt file in wwwroot/SeedData
        public const string CacheRoleName = "CacheRole";
        
        private readonly ExtraAuthorizeDbContext _context;

        public CacheRoleService(ExtraAuthorizeDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// This just shows the user's permissions which start with the string "Cache"
        /// </summary>
        /// <param name="usersClaims"></param>
        /// <returns></returns>
        public IEnumerable<Permissions> ShowExistingCachePermissions(IEnumerable<Claim> usersClaims)
        {
            return usersClaims.PermissionsFromClaims()?.Where(x => x.ToString().StartsWith("Cache"));
        }

        /// <summary>
        /// This toggles whether the <see cref="Permissions.Cache2"/> permission is in the <see cref="CacheRoleName"/>.
        /// This causes the <see cref="ExtraAuthorizeDbContext"/> to update the TimeStore with the time this change happens.
        /// Then the <see cref="CodeCalledInStartup.AuthCookieValidate"/> will compare the users lastUpdated time which will
        /// cause a recalc of the logged-in user's permission claim.
        /// </summary>
        public void ToggleCacheRole()
        {
            var hasCache2Permission = _context.Find<RoleToPermissions>(CacheRoleName)
                .PermissionsInRole.Any(x => x == Permissions.Cache2);
            var updatedPermissions = new List<Permissions> {Permissions.Cache1};
            if (!hasCache2Permission)
                updatedPermissions.Add(Permissions.Cache2);

            var authUserHelper = new ExtraAuthUsersSetup(_context);
            authUserHelper.UpdateRole(CacheRoleName, $"Has {updatedPermissions.Count} permissions.", updatedPermissions);
            _context.SaveChanges();
        }

        /// <summary>
        /// This allows us to see the time the last times
        /// 1. The last time the Role/Permissions were changes (database value)
        /// 2. The last time the current user's permissions were calculated (user claim)
        /// </summary>
        /// <param name="usersClaims"></param>
        /// <returns></returns>
        public IEnumerable<string> GetFeatureLastUpdated(IEnumerable<Claim> usersClaims)
        {
            var databaseValue = _context.Find<TimeStore>(AuthChangesConsts.FeatureCacheKey)?.LastUpdatedTicks;
            yield return databaseValue == null
                ? "No database value present"
                : $"Database: {new DateTime((long)databaseValue):F}";

            var claimsValue = usersClaims
                .SingleOrDefault(x => x.Type == PermissionConstants.LastPermissionsUpdatedClaimType)?.Value;
            yield return claimsValue == null
                ? "No claim value present"
                : $"User Claim:    {new DateTime(long.Parse(claimsValue)):F}";
        }
    }
}