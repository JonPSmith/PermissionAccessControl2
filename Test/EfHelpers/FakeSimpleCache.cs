// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using CommonCache;

namespace Test.EfHelpers
{
    public class FakeSimpleCache : ISimpleTimeCache
    {
        public long CachedValue { get; private set; } = -1;
        public bool AddOrUpdateCalled => CachedValue != -1;

        public bool GivenTicksIsLowerThanCachedTicks(object cacheKey, string ticksToCompareString)
        {
            throw new System.NotImplementedException();
        }

        public bool GivenTicksIsLowerThanCachedTicks(object cacheKey, long ticksToCompare)
        {
            throw new System.NotImplementedException();
        }

        public bool GetValueOrUseDefault(object cacheKey, long defaultValue, out long cachedValue)
        {
            throw new System.NotImplementedException();
        }

        public void AddOrUpdate(object cacheKey, long cachedValue)
        {
            CachedValue = cachedValue;
        }

        public void Clear()
        {
            CachedValue = -1;
        }
    }
}