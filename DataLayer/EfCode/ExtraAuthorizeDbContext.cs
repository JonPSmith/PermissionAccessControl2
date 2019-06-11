// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.AppClasses.MultiTenantParts;
using DataLayer.EfCode.Configurations;
using DataLayer.ExtraAuthClasses;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.EfCode
{
    public class ExtraAuthorizeDbContext : DbContext
    {
        public DbSet<UserToRole> UserToRoles { get; set; }
        public DbSet<RoleToPermissions> RolesToPermissions { get; set; }
        public DbSet<ModulesForUser> ModulesForUsers { get; set; }
        
        //Now links to two classes in the AppDbContext that hold data used to set up the user's modules and data access rights
        public DbSet<TenantBase> Tenants { get; set; }
        public DbSet<UserDataHierarchical> DataAccess { get; set; }

        public ExtraAuthorizeDbContext(DbContextOptions<ExtraAuthorizeDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ExtraAuthorizeConfig();
        }
    }
}