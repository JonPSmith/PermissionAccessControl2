using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using CommonCache;
using DataLayer.EfCode;
using PermissionParts;
using ServiceLayer.UserServices.Internal;

namespace ServiceLayer.UserServices.Concrete
{
    public class CacheRoleService : ICacheRoleService
    {
        private readonly ExtraAuthorizeDbContext _context;

        public CacheRoleService(ExtraAuthorizeDbContext context)
        {
            _context = context;
        }


        public IEnumerable<Permissions> ShowExistingCachePermissions(IEnumerable<Claim> usersClaims)
        {
            return usersClaims.PermissionsFromClaims()?.Where(x => x.ToString().StartsWith("Cache"));
        }

        public void ToggleCacheRole(IEnumerable<Claim> usersClaims)
        {
            var hasCache2Permission = usersClaims.PermissionsFromClaims().Any(x => x == Permissions.Cache2);
            var updatedPermissions = new List<Permissions> {Permissions.Cache1};
            if (!hasCache2Permission)
                updatedPermissions.Add(Permissions.Cache2);

            var authUserHelper = new ExtraAuthUsersSetup(_context);
            authUserHelper.UpdateRole("CacheRole", updatedPermissions);
            _context.SaveChanges();
        }
    }
}