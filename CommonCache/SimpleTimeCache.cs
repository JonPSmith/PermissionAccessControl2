using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;

namespace CommonCache
{
    public class SimpleTimeCache : ISimpleTimeCache
    {
        private static readonly ConcurrentDictionary<object, int> StaticCache = new ConcurrentDictionary<object, int>();

        public bool GivenTicksIsHigherThanCachedTicks(object cacheKey, string ticksToCompareString)
        {
            if (ticksToCompareString == null) return true;
            var ticksToCompare = long.Parse(ticksToCompareString);
            return GivenTicksIsHigherThanCachedTicks(cacheKey, ticksToCompare);
        }

        public bool GivenTicksIsHigherThanCachedTicks(object cacheKey, long ticksToCompare)
        {
            return StaticCache.TryGetValue(cacheKey, out var cachedTicks) && (ticksToCompare > cachedTicks);
        }

        public bool GetValueOrUseDefault(object cacheKey, int defaultValue, out int cachedValue)
        {
            if (StaticCache.TryGetValue(cacheKey, out cachedValue))
            {
                cachedValue = defaultValue;
                return false;
            }

            return true;
        }

        public void AddOrUpdate(object cacheKey, int cachedValue)
        {
            StaticCache.AddOrUpdate(cacheKey, cachedValue, (key, value) => cachedValue);
        }

        public void Clear()
        {
            StaticCache.Clear();
        }
    }
}
