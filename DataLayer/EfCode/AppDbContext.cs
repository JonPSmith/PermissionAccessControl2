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
        private readonly string _userId;
        private readonly string _dataKey;

        public DbSet<GeneralNote> GeneralNotes { get; set; }
        public DbSet<PersonalData> PersonalDatas { get; set; }
        public DbSet<TenantBase> TenantItems { get; set; }
        public DbSet<ShopStock> ShopStocks { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options, IGetClaimsProvider claimsProvider)
            : base(options)
        {
            _userId = claimsProvider.UserId;
            _dataKey = claimsProvider.DataKey;
        }

        //I only have to override these two version of SaveChanges, as the other two SaveChanges versions call these
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.MarkWithUserIdIfNeeded(_userId);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            this.MarkWithUserIdIfNeeded(_userId);
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AppConfig(_userId, _dataKey);
        }
    }
}