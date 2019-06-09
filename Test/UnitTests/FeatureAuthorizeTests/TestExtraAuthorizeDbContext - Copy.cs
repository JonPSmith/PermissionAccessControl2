// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using DataLayer.EfCode;
using DataLayer.ExtraAuthClasses;
using PermissionParts;
using Test.EfHelpers;
using TestSupport.EfHelpers;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.FeatureAuthorizeTests
{
    public class TestCalcAllowedPermissions
    {
        [Fact]
        public void TestSeedUserWithTwoRoles()
        {
            //SETUP
            var options = SqliteInMemory.CreateOptions<ExtraAuthorizeDbContext>();
            using (var context = new ExtraAuthorizeDbContext(options))
            {
                context.Database.EnsureCreated();
                context.SeedUserWithTwoRoles();

                //ATTEMPT

                //VERIFY
                
            }
        }

        

    }
}