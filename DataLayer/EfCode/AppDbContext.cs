// Copyright (c) 2018 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataAuthorize;
using DataLayer.AppClasses;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using DataLayer.AppClasses.MultiTenantParts;
using DataLayer.EfCode.Configurations;

namespace DataLayer.EfCode
{
    public class AppDbContext : DbContext
    {
        internal readonly string UserId;
        internal readonly string DataKey;

        public DbSet<GeneralNote> GeneralNotes { get; set; }
        public DbSet<PersonalData> PersonalDatas { get; set; }
        public DbSet<TenantBase> TenantItems { get; set; }
        public DbSet<ShopStock> ShopStocks { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options, IGetClaimsProvider claimsProvider)
            : base(options)
        {
            UserId = claimsProvider.UserId;
            DataKey = claimsProvider.DataKey;
        }

        //I only have to override these two version of SaveChanges, as the other two SaveChanges versions call these
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.MarkWithDataKeyIfNeeded(UserId, DataKey);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            this.MarkWithDataKeyIfNeeded(UserId, DataKey);
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AppConfig(this);
        }
    }
}