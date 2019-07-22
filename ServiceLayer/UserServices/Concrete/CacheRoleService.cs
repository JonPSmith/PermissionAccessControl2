using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using CommonCache;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses;
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
        private readonly IDistributedCache _cache;

        public CacheRoleService(ExtraAuthorizeDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
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

        public IEnumerable<string> GetFeatureLastUpdated()
        {
            var databaseValue = _context.Find<TimeStore>(AuthChangesConsts.FeatureCacheKey)?.Value;
            yield return databaseValue == null
                ? "No database value present"
                : $"Database: {new DateTime(BitConverter.ToInt64(databaseValue, 0)):F}";

            var cacheValue = _cache.Get(AuthChangesConsts.FeatureCacheKey);
            yield return cacheValue == null
                ? "No cache value present"
                : $"Cache:    {new DateTime(BitConverter.ToInt64(cacheValue, 0)):F}";
        }
    }
}