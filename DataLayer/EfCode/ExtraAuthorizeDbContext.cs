// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.EfCode.Configurations;
using DataLayer.ExtraAuthClasses;
using DataLayer.ExtraAuthClasses.Support;
using DataLayer.MultiTenantClasses;
using Microsoft.EntityFrameworkCore;
using RefreshClaimsParts;

namespace DataLayer.EfCode
{
    public class ExtraAuthorizeDbContext : DbContext, ITimeStore
    {
        private readonly IAuthChanges _authChange;

        public DbSet<UserToRole> UserToRoles { get; set; }
        public DbSet<RoleToPermissions> RolesToPermissions { get; set; }
        public DbSet<ModulesForUser> ModulesForUsers { get; set; }

        /// <summary>
        /// The TimeStore holds the time when a change is made to the Roles/Permission such that it might alter a user's permissions.
        /// //YOU ONLY NEED THIS IF YOU WANT THE "REFRESH USER CLAIMS" FEATURE
        /// </summary>
        public DbSet<TimeStore> TimeStores { get; set; }

        //YOU ONLY NEED THESE TWO DBSET'S IF YOU WANT THE Hierarchical DATAKEY FEATURE
        //Now links to two classes in the CompanyDbContext that hold data used to set up the user's modules and data access rights
        public DbSet<TenantBase> Tenants { get; set; }
        public DbSet<UserDataHierarchical> DataAccess { get; set; }

        //YOU ONLY NEED TO OVERRIDE SAVECHANGES IF YOU WANT THE "REFRESH USER CLAIMS" FEATURE
        //I only have to override these two versions of SaveChanges, as the other two SaveChanges versions call these
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        { 
            if (_authChange == null)
                //_authChange is null if not using UpdateCookieOnChange, so bypass permission change code
                return base.SaveChanges(acceptAllChangesOnSuccess);

            if (this.UserPermissionsMayHaveChanged())
                _authChange.AddOrUpdate(this);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_authChange == null)
                //_authChange is null if not using UpdateCookieOnChange, so bypass permission change code
                return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            if (this.UserPermissionsMayHaveChanged())
                _authChange?.AddOrUpdate(this);
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
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

        //-------------------------------------------------------
        //The ITimeStore methods


        public long? GetValueFromStore(string key)
        {
            return Find<TimeStore>(key)?.LastUpdatedTicks;
        }

        public void AddUpdateValue(string key, long cachedTicks)
        {
            var currentEntry = Find<TimeStore>(key);
            if (currentEntry != null)
            {
                currentEntry.LastUpdatedTicks = cachedTicks;
            }
            else
            {
                Add(new TimeStore {Key = key, LastUpdatedTicks = cachedTicks });
            }
        }
    }
}