// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses;
using Microsoft.EntityFrameworkCore;

namespace ServiceLayer.CodeCalledInStartup
{
    public class CalcDataKey

    {
        /// <summary>
        /// NOTE: This class is used in OnValidatePrincipal so it can't use DI, so I can't inject the DbContext here because that is dynamic.
        /// Therefore I can pass in the database options because that is a singleton
        /// From that the method can create a valid dbContext to access the database
        /// </summary>
        private readonly DbContextOptions<ExtraAuthorizeDbContext> _extraAuthDbContextOptions;

        private readonly ExtraAuthorizeDbContext _extraContext;

        public CalcDataKey(ExtraAuthorizeDbContext extraContext)
        {
            _extraContext = extraContext;
        }

        public CalcDataKey(DbContextOptions<ExtraAuthorizeDbContext> extraAuthDbContextOptions)
        {
            _extraAuthDbContextOptions = extraAuthDbContextOptions;
        }

        /// <summary>
        /// This looks for a DataKey for the current user, which can be missing
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>The found data key, or empty string if not found</returns>
        public string CalcDataKeyForUser(string userId)
        {
            var extraContext = GetExtraAuthContext();
            var tenantInfo = extraContext.Find<UserDataHierarchical>(userId);
            if (tenantInfo != null)
            {
                var foundTenant = extraContext.Tenants.Find(tenantInfo.LinkedTenantId);
                return foundTenant?.DataKey ?? string.Empty;
            }

            return string.Empty;
        }

        private ExtraAuthorizeDbContext GetExtraAuthContext()
        {
            return _extraContext ?? new ExtraAuthorizeDbContext(_extraAuthDbContextOptions, null);
        }
    }
}