// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommonCache;
using DataLayer.EfCode.Configurations;
using DataLayer.ExtraAuthClasses;
using DataLayer.ExtraAuthClasses.Support;
using DataLayer.MultiTenantClasses;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.EfCode
{
    public class ExtraAuthorizeDbContext : DbContext, ITimeStore
    {
        private readonly IAuthChanges _cache;

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
            var changed = this.UserPermissionsMayHaveChanged();
            var result = base.SaveChanges(acceptAllChangesOnSuccess);
            //We log this after the SaveChange was successful
            if (changed)
                _cache?.AddOrUpdate(AuthChangesConsts.FeatureCacheKey, DateTime.UtcNow.Ticks);
            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            var changed = this.UserPermissionsMayHaveChanged();
            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            //We log this after the SaveChange was successful
            if (changed)
                _cache?.AddOrUpdate(AuthChangesConsts.FeatureCacheKey, DateTime.UtcNow.Ticks);
            return result;
        }

        public ExtraAuthorizeDbContext(DbContextOptions<ExtraAuthorizeDbContext> options, IAuthChangesFactory cache)
            : base(options)
        {
            _cache = cache;
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