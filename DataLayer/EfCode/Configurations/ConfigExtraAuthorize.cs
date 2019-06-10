// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using DataAuthorize;
using DataLayer.AppClasses;
using DataLayer.AppClasses.MultiTenantParts;
using DataLayer.ExtraAuthClasses;
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
        public static void ExtraAuthorizeConfig(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserToRole>().HasKey(x => new { x.UserId, x.RoleName });
        }

        public static void AppConfig(this ModelBuilder modelBuilder, string userId, string dataKey)
        {
            AddQueryFilterAndIndex(modelBuilder.Entity<PersonalData>(), x => x.DataKey == userId);
            AddQueryFilterAndIndex(modelBuilder.Entity<TenantBase>(), x => x.DataKey.StartsWith(dataKey));
            AddQueryFilterAndIndex(modelBuilder.Entity<ShopStock>(), x => x.DataKey.StartsWith(dataKey));
        }

        /// <summary>
        /// This applies the correct query filter to the entity based on its interface type
        /// NOTE: OnModelCreating is only run once and the results cached so you can't dynamically change filters.
        ///       If you need dynamic query filters you need to build it into the lambda 
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="filter"></param>
        private static void AddQueryFilterAndIndex<T>(EntityTypeBuilder<T> builder, Expression<Func<T, bool>> filter) where T : class, IDataKey
        {
            builder.HasQueryFilter(filter);
            //add an index to make the filter quicker
            builder.HasIndex(nameof(IDataKey.DataKey));
        }
    }
}