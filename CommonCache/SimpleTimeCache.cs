using System.Collections.Concurrent;

namespace CommonCache
{
    public class SimpleTimeCache : ISimpleTimeCache
    {
        public const string FeatureCacheKey = "8FA8E7A8-0ADD-433C-B063-3BD725254C9B";

        private static readonly ConcurrentDictionary<object, long> StaticCache = new ConcurrentDictionary<object, long>();

        /// <summary>
        /// This returns true if there is an entry in the cache and the ticks given are lower, i.e. we need to recalc things
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="ticksToCompareString"></param>
        /// <returns></returns>
        public bool GivenTicksIsLowerThanCachedTicks(object cacheKey, string ticksToCompareString)
        {
            if (ticksToCompareString == null) return false;
            var ticksToCompare = long.Parse(ticksToCompareString);
            return GivenTicksIsLowerThanCachedTicks(cacheKey, ticksToCompare);
        }

        public bool GivenTicksIsLowerThanCachedTicks(object cacheKey, long ticksToCompare)
        {
            return StaticCache.TryGetValue(cacheKey, out var cachedTicks) && (ticksToCompare < cachedTicks);
        }

        public bool GetValueOrUseDefault(object cacheKey, long defaultValue, out long cachedValue)
        {
            if (!StaticCache.TryGetValue(cacheKey, out cachedValue))
            {
                cachedValue = defaultValue;
                return false;
            }

            return true;
        }

        public void AddOrUpdate(object cacheKey, long cachedValue)
        {
            StaticCache.AddOrUpdate(cacheKey, cachedValue, (key, value) => cachedValue);
        }

        public void Clear()
        {
            StaticCache.Clear();
        }
    }
}
