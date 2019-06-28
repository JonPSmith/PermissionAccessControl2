// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Linq;
using PermissionParts;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.SecurityChecks
{
    public class CheckPermissions
    {
        //Its VERY important that you don't have duplicate Permission numbers as it could cause a security breach
        [Fact]
        public void CheckPermissionsHaveUniqueNumberOk()
        {
            //SETUP

            //ATTEMPT    
            var nums = Enum.GetValues(typeof(Permissions)).Cast<Permissions>().Select(x => (int)x).ToList(); 

            //VERIFY
            nums.Count.ShouldEqual(nums.Distinct().Count());
        }
    }
}