// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataKeyParts;
using DataLayer.ExtraAuthClasses;
using DataLayer.MultiTenantClasses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataLayer.EfCode.Configurations
{
    /// <summary>
    /// I need to place the configs for the databases in one place because I use context.Database.EnsureCreated to create it.
    /// This is only for a demo app - I would normally do this via SQL scripts and EFSchemaCompare
    /// https://www.thereformedprogrammer.net/handling-entity-framework-core-database-migrations-in-production-part-1/#2b-hand-coding-sql-migration-scripts
    /// </summary>
    public static class ConfigExtensions
    {
        public static void TenantBaseConfig(this ModelBuilder modelBuilder)
        {
            //for some reason ExtraAuthorizeConfig doesn't config this properly so I need to add this
            modelBuilder.Entity<TenantBase>()
                .HasDiscriminator<string>("TenantType")
                .HasValue<Company>(nameof(Company))
                .HasValue<SubGroup>(nameof(SubGroup))
                .HasValue<RetailOutlet>(nameof(RetailOutlet));

            //This is needed in version 2.2 to make the _children collection work, but isn't needed in EF Core 3
            //see https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.0/breaking-changes#backing-fields-are-used-by-default
            modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);
        }

        public static void ExtraAuthorizeConfig(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserToRole>().HasKey(x => new { x.UserId, x.RoleName });

            modelBuilder.Entity<RoleToPermissions>()
                .Property("_permissionsInRole")
                .HasColumnName("PermissionsInRole");
        }

        public static void CompanyDbConfig(this ModelBuilder modelBuilder, CompanyDbContext context)
        {
            AddHierarchicalQueryFilter(modelBuilder.Entity<TenantBase>(), context);
            AddHierarchicalQueryFilter(modelBuilder.Entity<ShopStock>(), context);
            AddHierarchicalQueryFilter(modelBuilder.Entity<ShopSale>(), context);
        }

        private static void AddHierarchicalQueryFilter<T>(EntityTypeBuilder<T> builder, CompanyDbContext context) where T : class, IDataKey
        {
            builder.HasQueryFilter(x => x.DataKey.StartsWith(context.DataKey));
            builder.HasIndex(x => x.DataKey);
        }
    }
}