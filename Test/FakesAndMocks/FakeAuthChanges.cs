// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using CommonCache;

namespace Test.FakesAndMocks
{
    public class FakeAuthChanges : IAuthChanges
    {
        public long CachedValue { get; private set; } = -1;
        public bool CacheValueSet => CachedValue != -1;

        public bool IsOutOfDateOrMissing(string cacheKey, string ticksToCompareString, ITimeStore timeStore)
        {
            throw new System.NotImplementedException();
        }

        public bool IsLowerThan(string cacheKey, long ticksToCompare)
        {
            throw new System.NotImplementedException();
        }

        public void AddOrUpdate(string cacheKey, long cachedValue, ITimeStore timeStore)
        {
            CachedValue = cachedValue;
        }

        public void Clear()
        {
            CachedValue = -1;
        }
    }
}