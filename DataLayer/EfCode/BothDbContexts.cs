// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.AppClasses;
using DataLayer.AppClasses.MultiTenantParts;
using DataLayer.EfCode.Configurations;
using DataLayer.ExtraAuthClasses;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.EfCode
{
    /// <summary>
    /// This only exists to create one database that covers both ExtraAuthorizeDbContext and AppDbContext
    /// </summary>
    public class BothDbContexts : DbContext
    {
        //ExtraAuthorizeDbContext
        public DbSet<UserToRole> UserToRoles { get; set; }
        public DbSet<RoleToPermissions> RolesToPermissions { get; set; }
        public DbSet<UserDataAccessBase> DataAccess { get; set; }
        public DbSet<ModulesForUser> ModulesForUsers { get; set; }

        //AppDbContext
        public DbSet<GeneralNote> GeneralNotes { get; set; }
        public DbSet<PersonalData> PersonalDatas { get; set; }
        public DbSet<TenantBase> TenantItems { get; set; }
        public DbSet<ShopStock> ShopStocks { get; set; }

        public BothDbContexts(DbContextOptions<BothDbContexts> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ExtraAuthorizeConfig();
            modelBuilder.AppConfig(null, null);
        }
    }
}