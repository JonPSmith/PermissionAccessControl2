// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Distributed;

[assembly: InternalsVisibleTo("Test")]

namespace CommonCache
{
    public class AuthChanges : IAuthChanges
    {
        private readonly IDistributedCache _cache;
        private readonly ITimeStore _databaseAccess;

        internal AuthChanges(IDistributedCache cache, ITimeStore databaseAccess)
        {
            _cache = cache;
            _databaseAccess = databaseAccess;
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
            {
                //not in cache, so read from TimeStore in database
                bytes = _databaseAccess.GetValueFromStore(cacheKey);
                if (bytes == null)
                    throw new ApplicationException($"You must seed the database with a cache value for the key {cacheKey}.");
            }
            var cachedTicks = BitConverter.ToInt64(bytes, 0);
            return ticksToCompare < cachedTicks;
        }

        public void AddOrUpdate(string cacheKey, long cachedValue)
        {
            var bytes = BitConverter.GetBytes(cachedValue);
            _databaseAccess.AddUpdateValue(cacheKey, bytes);
            _cache.Set(cacheKey, bytes);
        }
    }
}
