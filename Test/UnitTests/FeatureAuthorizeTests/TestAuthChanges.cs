// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Castle.Components.DictionaryAdapter;
using CommonCache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Test.FakesAndMocks;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.FeatureAuthorizeTests
{
    public class TestAuthChanges
    {

        [Theory]
        [InlineData("test", "123", true)]
        [InlineData("test", "1234", false)]
        [InlineData("test", null, true)]
        public void TestIsOutOfDateOrMissing(string key, string ticksToTry, bool expectedResult )
        {
            //SETUP
            var fakeTimeStore = new FakeTimeStore();
            var authChange = new AuthChanges();
            authChange.AddOrUpdate("test", 200, fakeTimeStore);

            //ATTEMPT
            var isOutOfDate = authChange.IsOutOfDateOrMissing(key, ticksToTry, fakeTimeStore);

            //VERIFY
            isOutOfDate.ShouldEqual(expectedResult);
        }

        [Fact]
        public void TestIsOutOfDateOrMissingNoOriginalValue()
        {
            //SETUP
            var fakeTimeStore = new FakeTimeStore()
            {
                Value = BitConverter.GetBytes((long)200)
            };
            var authChange = new AuthChanges();

            //ATTEMPT
            var isOutOfDate = authChange.IsOutOfDateOrMissing("test", "100", fakeTimeStore);

            //VERIFY
            isOutOfDate.ShouldEqual(true);
            fakeTimeStore.Key.ShouldNotBeNull();
        }

        [Fact]
        public void TestAddOrUpdateDatabaseUpdate()
        {
            //SETUP
            var fakeTimeStore = new FakeTimeStore()
            {
                Value = BitConverter.GetBytes((long)200)
            };
            var authChange = new AuthChanges();

            //ATTEMPT
            authChange.AddOrUpdate("test", 100, fakeTimeStore);

            //VERIFY
            fakeTimeStore.Value.ShouldEqual(BitConverter.GetBytes((long)100));
        }

        [Fact]
        public void TestAddOrUpdateDatabaseAdd()
        {
            //SETUP
            var fakeTimeStore = new FakeTimeStore();
            var authChange = new AuthChanges();

            //ATTEMPT
            authChange.AddOrUpdate("test", 100, fakeTimeStore);

            //VERIFY
            fakeTimeStore.Value.ShouldEqual(BitConverter.GetBytes((long)100));
        }

    }
}