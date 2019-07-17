// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using CommonCache;

namespace Test.EfHelpers
{
    public class FakeAuthChanges : IAuthChanges
    {
        public long CachedValue { get; private set; } = -1;
        public bool CacheValueSet => CachedValue != -1;

        public bool IsLowerThan(string cacheKey, string ticksToCompareString)
        {
            throw new System.NotImplementedException();
        }

        public bool IsLowerThan(string cacheKey, long ticksToCompare)
        {
            throw new System.NotImplementedException();
        }

        public Action AddOrUpdate(string cacheKey, long cachedValue)
        {
            CachedValue = cachedValue;
            return () => { };
        }

        public void Clear()
        {
            CachedValue = -1;
        }
    }
}