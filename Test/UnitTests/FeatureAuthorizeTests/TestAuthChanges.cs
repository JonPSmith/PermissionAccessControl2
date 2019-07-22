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
        [Fact]
        public void TestIDistributedCache()
        {
            //SETUP
            var cache = new MemoryDistributedCache(new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions()));
            cache.Get("test").ShouldBeNull();

            //ATTEMPT
            cache.Set("test", new byte[]{1});
            var result = cache.Get("test");

            //VERIFY
            result.ShouldEqual(new byte[] { 1 });
        }

        [Theory]
        [InlineData("test", "123", true)]
        [InlineData("test", "1234", false)]
        [InlineData("test", null, true)]
        public void TestIsOutOfDateOrMissing(string key, string ticksToTry, bool expectedResult )
        {
            //SETUP
            var fakeTimeStore = new FakeTimeStore();
            var cache = new AuthChanges(new FakeDistributedCache());
            cache.AddOrUpdate("test", 200, fakeTimeStore);

            //ATTEMPT
            var isOutOfDate = cache.IsOutOfDateOrMissing(key, ticksToTry, () => fakeTimeStore);

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
            var cache = new AuthChanges(new FakeDistributedCache());

            //ATTEMPT
            var isOutOfDate = cache.IsOutOfDateOrMissing("test", "100", () => fakeTimeStore);

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
            var fakeCache = new FakeDistributedCache();
            var cache = new AuthChanges(fakeCache);

            //ATTEMPT
            var action = cache.AddOrUpdate("test", 100, fakeTimeStore);

            //VERIFY
            fakeTimeStore.Value.ShouldEqual(BitConverter.GetBytes((long)100));
        }

        [Fact]
        public void TestAddOrUpdateDatabaseAdd()
        {
            //SETUP
            var fakeTimeStore = new FakeTimeStore();
            var fakeCache = new FakeDistributedCache();
            var cache = new AuthChanges(fakeCache);

            //ATTEMPT
            var action = cache.AddOrUpdate("test", 100, fakeTimeStore);

            //VERIFY
            fakeTimeStore.Value.ShouldEqual(BitConverter.GetBytes((long)100));
        }

        [Fact]
        public void TestAddOrUpdateCacheUpdate()
        {
            //SETUP
            var fakeTimeStore = new FakeTimeStore()
            {
                Value = BitConverter.GetBytes((long)200)
            };
            var fakeCache = new FakeDistributedCache();
            var cache = new AuthChanges(fakeCache);

            //ATTEMPT
            var action = cache.AddOrUpdate("test", 100, fakeTimeStore);
            action.Invoke();

            //VERIFY
            fakeCache.CachedValue.ShouldEqual(BitConverter.GetBytes((long)100));
        }

    }
}