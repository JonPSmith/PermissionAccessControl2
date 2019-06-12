// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using CommonCache;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.FeatureAuthorizeTests
{
    public class TestSimpleCache
    {
        [Theory]
        [InlineData("test", 12345)]
        [InlineData("badkey", -1)]
        public void TestSetGet(string key, int expectedValue)
        {
            //SETUP
            var cache = new SimpleTimeCache();
            cache.Clear();
            cache.AddOrUpdate("test", 12345);

            //ATTEMPT
            cache.GetValueOrUseDefault(key, -1, out var result);

            //VERIFY
            result.ShouldEqual(expectedValue);
        }

        [Theory]
        [InlineData("test", "123", false)]
        [InlineData("test", "1234", true)]
        [InlineData("test", null, true)]
        [InlineData("badkey", "1234", false)]

        public void TestGivenTicksIsHigherThanCachedTicks(string key, string ticksToTry, bool expectedResult )
        {
            //SETUP
            var cache = new SimpleTimeCache();
            cache.Clear();
            cache.AddOrUpdate("test", 200);

            //ATTEMPT
            var isHigher = cache.GivenTicksIsHigherThanCachedTicks(key, ticksToTry);

            //VERIFY
            isHigher.ShouldEqual(expectedResult);
        }
    }
}