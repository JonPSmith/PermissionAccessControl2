// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfCode;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.FeatureAuthorizeTests
{
    public class TestTimeStore
    {
        private ITestOutputHelper _output;

        public TestTimeStore(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void TestTestAddUpdateNoExistingKey()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, null))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                context.AddUpdateValue("test", (long)1234);
                context.SaveChanges();

                //VERIFY
                context.TimeStores.Single().LastUpdatedTicks.ShouldEqual((long)1234);
            }
        }

        [Fact]
        public void TestTestAddUpdateExistingKey()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, null))
            {
                context.Database.EnsureCreated();
                context.AddUpdateValue("test", (long)1234);
                context.SaveChanges();

                //ATTEMPT
                context.AddUpdateValue("test", (long)5678);
                context.SaveChanges();

                //VERIFY
                context.TimeStores.Single().LastUpdatedTicks.ShouldEqual((long)5678);
            }
        }

        [Fact]
        public void TestGetValueFromStore()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, null))
            {
                context.Database.EnsureCreated();
                context.AddUpdateValue("test", (long)1234);
                context.SaveChanges();

                //ATTEMPT
                var result = context.GetValueFromStore("test");

                //VERIFY
                result.ShouldEqual((long)1234);
            }
        }

        [Fact]
        public void TestGetValueFromStoreNoContent()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, null))
            {
                context.Database.EnsureCreated();

                //ATTEMPT
                var result = context.GetValueFromStore("test");

                //VERIFY
                result.ShouldBeNull();
            }
        }

        [Fact]
        public void GetValueFromStorePerformance()
        {
            //SETUP
            var options = this.CreateUniqueClassOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options, null))
            {
                context.Database.EnsureCreated();
                context.AddUpdateValue("test", (long)1234);
                context.SaveChanges();

                const int numTimes = 1000;
                //ATTEMPT

                using (new TimeThings(_output, "EFCore - startup", 10))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var result = context.GetValueFromStore("test");
                    }
                }
                using (new TimeThings(_output, "EFCore", numTimes))
                {
                    for (int i = 0; i < numTimes; i++)
                    {
                        var result = context.GetValueFromStore("test");
                    }
                }
                using (new TimeThings(_output, "EFCore", numTimes))
                {
                    for (int i = 0; i < numTimes; i++)
                    {
                        var result = context.GetValueFromStore("test");
                    }
                }

            }
        }

    }
}