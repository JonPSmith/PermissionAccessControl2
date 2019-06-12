// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using PermissionParts;

[assembly: InternalsVisibleTo("Test")]

namespace ServiceLayer.CodeCalledInStartup
{
    public class CalcAllowedPermissions : IDisposable
    {
        /// <summary>
        /// NOTE: This class is used in OnValidatePrincipal so it can't use DI, so I can't inject the DbContext here because that is dynamic.
        /// Therefore I can pass in the database options because that is a singleton
        /// From that the method can create a valid dbContext to access the database
        /// </summary>
        private readonly DbContextOptions<ExtraAuthorizeDbContext> _extraAuthDbContextOptions;

        private ExtraAuthorizeDbContext _context;

        public CalcAllowedPermissions(ExtraAuthorizeDbContext context)
        {
            _context = context;
        }

        public CalcAllowedPermissions(DbContextOptions<ExtraAuthorizeDbContext> extraAuthDbContextOptions)
        {
            _extraAuthDbContextOptions = extraAuthDbContextOptions;
        }

        /// <summary>
        /// This is called if the Permissions that a user needs calculating.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<string> CalcPermissionsForUser(string userId)
        {
            var dbContext = GetContext();
            //This gets all the permissions, with a distinct to remove duplicates
            var permissionsForUser = await dbContext.UserToRoles.Where(x => x.UserId == userId)
                .SelectMany(x => x.Role.PermissionsInRole)
                .Distinct()
                .ToListAsync();
            //we get the modules this user is allowed to see
            var userModules =
                dbContext.ModulesForUsers.Find(userId)
                    ?.AllowedPaidForModules ?? PaidForModules.None;
            //Now we remove permissions that are linked to modules that the user has no access to
            var filteredPermissions =
                from permission in permissionsForUser
                let moduleAttr = typeof(Permissions).GetMember(permission.ToString())[0]
                    .GetCustomAttribute<LinkedToModuleAttribute>()
                where moduleAttr == null || userModules.HasFlag(moduleAttr.PaidForModule)
                select permission;

            return filteredPermissions.PackPermissionsIntoString();
        }

        private ExtraAuthorizeDbContext GetContext()
        {
            return _context ?? new ExtraAuthorizeDbContext(_extraAuthDbContextOptions);
        }

        #region IDisposable Support

        protected virtual void Dispose(bool disposing)
        {
            if (_extraAuthDbContextOptions != null && _context != null)
            {
                if (disposing)
                {
                    _context.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                _context = null;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~CalcAllowedPermissions() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }

        #endregion
    }
}