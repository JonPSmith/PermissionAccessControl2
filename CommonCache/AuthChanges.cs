// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.Caching.Distributed;

namespace CommonCache
{
    public class AuthChanges : IAuthChanges
    {
        public const string FeatureCacheKey = "8FA8E7A8-0ADD-433C-B063-3BD725254C9B";

        private IDistributedCache _cache;

        public AuthChanges(IDistributedCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// This returns true if there is an entry in the cache and the ticks given are lower, i.e. we need to recalc things
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="ticksToCompareString"></param>
        /// <returns></returns>
        public bool IsLowerThan(string cacheKey, string ticksToCompareString)
        {
            if (ticksToCompareString == null) return false;
            var ticksToCompare = long.Parse(ticksToCompareString);
            return IsLowerThan(cacheKey, ticksToCompare);
        }

        private bool IsLowerThan(string cacheKey, long ticksToCompare)
        {
            var bytes = _cache.Get(cacheKey);
            if (bytes == null)
                throw new ApplicationException("Replace with sql read");
            var cachedTicks = BitConverter.ToInt64(bytes, 0);
            return ticksToCompare < cachedTicks;
        }

        public void AddOrUpdate(string cacheKey, long cachedValue)
        {
            var bytes = BitConverter.GetBytes(cachedValue);
            _cache.Set(cacheKey, bytes);
        }
    }
}
