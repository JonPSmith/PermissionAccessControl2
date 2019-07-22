// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

[assembly: InternalsVisibleTo("Test")]

namespace CommonCache
{
    public class AuthChanges : IAuthChanges
    {
        private readonly IDistributedCache _cache;

        public AuthChanges(IDistributedCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// This returns true if there is an entry in the cache and the ticks given are lower, i.e. we need to recalc things
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="ticksToCompareString"></param>
        /// <param name="getTimeStore">This func gets the timeStore - Func used to allow lazy creation of DbContext</param>
        /// <returns></returns>
        public bool IsLowerThan(string cacheKey, string ticksToCompareString, Func<ITimeStore> getTimeStore)
        {
            if (ticksToCompareString == null) return false;
            var ticksToCompare = long.Parse(ticksToCompareString);
            return IsLowerThan(cacheKey, ticksToCompare, getTimeStore.Invoke());
        }

        private bool IsLowerThan(string cacheKey, long ticksToCompare, ITimeStore databaseAccess)
        {
            var bytes = _cache.Get(cacheKey);
            if (bytes == null)
            {
                //not in cache, so read from TimeStore in database
                bytes = databaseAccess.GetValueFromStore(cacheKey);
                if (bytes == null)
                    throw new ApplicationException($"You must seed the database with a cache value for the key {cacheKey}.");
            }
            var cachedTicks = BitConverter.ToInt64(bytes, 0);
            return ticksToCompare < cachedTicks;
        }

        public Action AddOrUpdate(string cacheKey, long cachedValue, ITimeStore databaseAccess)
        {
            var bytes = BitConverter.GetBytes(cachedValue);
            databaseAccess.AddUpdateValue(cacheKey, bytes);
            return () => _cache.Set(cacheKey, bytes);
        }
    }
}
