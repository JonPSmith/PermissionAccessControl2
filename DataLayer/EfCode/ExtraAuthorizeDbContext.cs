// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using CommonCache;
using DataLayer.EfCode.Configurations;
using DataLayer.ExtraAuthClasses;
using DataLayer.ExtraAuthClasses.Support;
using DataLayer.MultiTenantClasses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DataLayer.EfCode
{
    public class ExtraAuthorizeDbContext : DbContext, ITimeStore
    {
        private readonly IAuthChanges _authChange;

        public DbSet<UserToRole> UserToRoles { get; set; }
        public DbSet<RoleToPermissions> RolesToPermissions { get; set; }
        public DbSet<ModulesForUser> ModulesForUsers { get; set; }

        public DbSet<TimeStore> TimeStores { get; set; }

        //Now links to two classes in the CompanyDbContext that hold data used to set up the user's modules and data access rights
        public DbSet<TenantBase> Tenants { get; set; }
        public DbSet<UserDataHierarchical> DataAccess { get; set; }

        //I only have to override these two version of SaveChanges, as the other two SaveChanges versions call these
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        { 
            if (_authChange == null)
                //_authChange is null if not using UpdateCookieOnChange, so bypass permission change code
                return base.SaveChanges(acceptAllChangesOnSuccess);

            Action callOnSuccess = null; 
            if (this.UserPermissionsMayHaveChanged())
                callOnSuccess = _authChange.AddOrUpdate(AuthChangesConsts.FeatureCacheKey, DateTime.UtcNow.Ticks, this);
            var result = base.SaveChanges(acceptAllChangesOnSuccess);
            //If SaveChange was successful we call the cache success method
            callOnSuccess?.Invoke();
            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            if (_authChange == null)
                //_authChange is null if not using UpdateCookieOnChange, so bypass permission change code
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            Action callOnSuccess = null;
            if (this.UserPermissionsMayHaveChanged())
                callOnSuccess = _authChange?.AddOrUpdate(AuthChangesConsts.FeatureCacheKey, DateTime.UtcNow.Ticks, this);
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            //If SaveChange was successful we call the cache success method
            callOnSuccess?.Invoke();
            return result;
        }

        public ExtraAuthorizeDbContext(DbContextOptions<ExtraAuthorizeDbContext> options, IAuthChanges authChange)
            : base(options)
        {
            _authChange = authChange;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.TenantBaseConfig();
            modelBuilder.ExtraAuthorizeConfig();
        }


        public byte[] GetValueFromStore(string key)
        {
            return Find<TimeStore>(key)?.Value;
        }

        public void AddUpdateValue(string key, byte[] value)
        {
            var currentEntry = Find<TimeStore>(key);
            if (currentEntry != null)
            {
                currentEntry.Value = value;
            }
            else
            {
                Add(new TimeStore {Key = key, Value = value});
            }
        }
    }
}