// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using DataLayer.AppClasses;
using DataLayer.AppClasses.MultiTenantParts;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using ServiceLayer.CodeCalledInStartup;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.DataAuthorizeTests
{
    public class TestNoFilteringAndUserIdFiltering
    {
        [Fact]
        public void TestCreateValidDatabaseOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<AppDbContext>();
            using (var context = new AppDbContext(options, new FakeGetClaimsProvider("userId", "accessKey")))
            {
                //ATTEMPT
                context.Database.EnsureCreated();

                //VERIFY
            }
        }

        [Fact]
        public void TestGeneralNoteIsNotFilteredOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<AppDbContext>();
            using (var context = new AppDbContext(options, new FakeGetClaimsProvider("userId", "accessKey")))
            {
                context.Database.EnsureCreated();
                context.Add(new GeneralNote {Note = "Hello"});
                context.SaveChanges();

            }
            using (var context = new AppDbContext(options, new FakeGetClaimsProvider("DIFF-userId", "DIFF-accessKey")))
            {
                //ATTEMPT
                var notes = context.GeneralNotes.ToList();

                //VERIFY
                notes.Count.ShouldEqual(1);
            }
        }

        [Fact]
        public void TestMarkWithDataKeyIfNeededWorks()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<AppDbContext>();
            using (var context = new AppDbContext(options, new FakeGetClaimsProvider("userId", "accessKey")))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                var entity = new PersonalData {YourNote = "Hello"};
                context.Add(entity);
                context.SaveChanges();

                //VERIFY
                entity.DataKey.ShouldEqual("userId");
            }
        }

        [Fact]
        public async Task TestMarkWithDataKeyIfNeededWorkAsync()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<AppDbContext>();
            using (var context = new AppDbContext(options, new FakeGetClaimsProvider("userId", "accessKey")))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                var entity = new PersonalData { YourNote = "Hello" };
                context.Add(entity);
                await context.SaveChangesAsync();

                //VERIFY
                entity.DataKey.ShouldEqual("userId");
            }
        }

        [Fact]
        public void TestQueryFilterWorks()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<AppDbContext>();
            using (var context = new AppDbContext(options, new FakeGetClaimsProvider("userId", "accessKey*")))
            {
                context.Database.EnsureCreated();
                context.Add(new ShopStock { Name = "dress" });
                context.SaveChanges();

            }
            using (var context = new AppDbContext(options, new FakeGetClaimsProvider("DIFF-userId", "DIFF-accessKey*")))
            {
                //ATTEMPT
                var stocksFiltered = context.ShopStocks.ToList();
                var stocksNotFiltered = context.ShopStocks.IgnoreQueryFilters().ToList();

                //VERIFY
                stocksFiltered.Count.ShouldEqual(0);
                stocksNotFiltered.Count.ShouldEqual(1);
                stocksNotFiltered.First().DataKey.ShouldEqual("accessKey*");
            }
        }
    }
}