using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using CommonCache;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses;
using FeatureAuthorize;
using Microsoft.Extensions.Caching.Distributed;
using PermissionParts;
using ServiceLayer.UserServices.Internal;

namespace ServiceLayer.UserServices.Concrete
{
    public class CacheRoleService : ICacheRoleService
    {
        //NOTE: If you change this you need to change the Roles.txt file in wwwroot/SeedData
        public const string CacheRoleName = "CacheRole";
        
        private readonly ExtraAuthorizeDbContext _context;

        public CacheRoleService(ExtraAuthorizeDbContext context)
        {
            _context = context;
        }


        public IEnumerable<Permissions> ShowExistingCachePermissions(IEnumerable<Claim> usersClaims)
        {
            return usersClaims.PermissionsFromClaims()?.Where(x => x.ToString().StartsWith("Cache"));
        }

        public void ToggleCacheRole()
        {
            var hasCache2Permission = _context.Find<RoleToPermissions>(CacheRoleName)
                .PermissionsInRole.Any(x => x == Permissions.Cache2);
            var updatedPermissions = new List<Permissions> {Permissions.Cache1};
            if (!hasCache2Permission)
                updatedPermissions.Add(Permissions.Cache2);

            var authUserHelper = new ExtraAuthUsersSetup(_context);
            authUserHelper.UpdateRole(CacheRoleName, updatedPermissions);
            _context.SaveChanges();
        }

        public IEnumerable<string> GetFeatureLastUpdated(IEnumerable<Claim> usersClaims)
        {
            var databaseValue = _context.Find<TimeStore>(AuthChangesConsts.FeatureCacheKey)?.Value;
            yield return databaseValue == null
                ? "No database value present"
                : $"Database: {new DateTime(BitConverter.ToInt64(databaseValue, 0)):F}";

            var claimsValue = usersClaims
                .SingleOrDefault(x => x.Type == PermissionConstants.LastPermissionsUpdatedClaimType)?.Value;
            yield return claimsValue == null
                ? "No claim value present"
                : $"User Claim:    {new DateTime(long.Parse(claimsValue)):F}";
        }
    }
}