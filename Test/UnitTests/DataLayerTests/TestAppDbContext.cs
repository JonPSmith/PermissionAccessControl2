// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using DataLayer.EfCode;
using Test.FakesAndMocks;
using TestSupport.EfHelpers;
using Xunit;

namespace Test.UnitTests.DataLayerTests
{
    public class TestAppDbContext
    {
        [Fact]
        public void TestCreateValidDatabaseOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<AppDbContext>();
            using (var context = new AppDbContext(options, new FakeGetClaimsProvider("user-id", "accessKey")))
            {
                //ATTEMPT
                context.Database.EnsureCreated();

                //VERIFY
            }
        }
    }
}