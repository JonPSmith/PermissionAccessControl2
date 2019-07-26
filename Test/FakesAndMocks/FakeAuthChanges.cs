// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using CommonCache;

namespace Test.FakesAndMocks
{
    public class FakeAuthChanges : IAuthChanges
    {
        public long CachedValue { get; private set; } = -1;
        public bool CacheValueSet { get; private set; }

        public bool IsOutOfDateOrMissing(string cacheKey, string ticksToCompareString, ITimeStore timeStore)
        {
            throw new System.NotImplementedException();
        }

        public bool IsLowerThan(string cacheKey, long ticksToCompare)
        {
            throw new System.NotImplementedException();
        }

        public void AddOrUpdate(ITimeStore timeStore)
        {
            CacheValueSet = true;
        }

        public void Clear()
        {
            CachedValue = -1;
            CacheValueSet = false;
        }
    }
}