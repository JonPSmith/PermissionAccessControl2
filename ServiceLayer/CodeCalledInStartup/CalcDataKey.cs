// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.EfCode;
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
        private readonly DbContextOptions<AppDbContext> _appDbContextOptions;

        private ExtraAuthorizeDbContext _extraContext;
        private AppDbContext _appContext;

        public CalcDataKey(ExtraAuthorizeDbContext extraContext, AppDbContext appContext)
        {
            _extraContext = extraContext;
            _appContext = appContext;
        }

        public CalcDataKey(DbContextOptions<ExtraAuthorizeDbContext> extraAuthDbContextOptions, DbContextOptions<AppDbContext> appDbContextOptions)
        {
            _extraAuthDbContextOptions = extraAuthDbContextOptions;
            _appDbContextOptions = appDbContextOptions;
        }



        private ExtraAuthorizeDbContext GetExtraAuthContext()
        {
            return _extraContext ?? new ExtraAuthorizeDbContext(_extraAuthDbContextOptions);
        }

        private AppDbContext GetAppDbContext()
        {
            return _appContext ?? new AppDbContext(_appDbContextOptions, new FakeGetClaimsProvider("dummy", "dummy"));
        }
    }
}