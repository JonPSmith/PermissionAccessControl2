// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Linq;
using DataLayer.EfCode;
using Test.EfHelpers;
using Test.FakesAndMocks;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.SecurityChecks
{
    public class CheckEntitiesAreSecure
    {
        [Fact]
        public void CheckQueryFiltersAreAppliedToEntityClassesOk()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<AppDbContext>();
            using (var context = new AppDbContext(options, new FakeGetClaimsProvider("user-id", "accessKey")))
            {
                var entities = context.Model.GetEntityTypes().ToList();

                //ATTEMPT
                var queryFilterErrs = entities.CheckEntitiesHasAQueryFilter().ToList();

                //VERIFY
                queryFilterErrs.Any().ShouldBeFalse(string.Join('\n', queryFilterErrs));
            }
        }
    }
}