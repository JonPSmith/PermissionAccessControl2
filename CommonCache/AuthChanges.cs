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
        /// <summary>
        /// This returns true if there is an entry in the cache and the ticks given are lower, i.e. we need to recalc things
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="ticksToCompareString"></param>
        /// <param name="timeStore"></param>
        /// <returns></returns>
        public bool IsOutOfDateOrMissing(string cacheKey, string ticksToCompareString, ITimeStore timeStore)
        {
            if (ticksToCompareString == null)
                //if there is no time claim then you do need to reset the claims
                return true;

            var ticksToCompare = long.Parse(ticksToCompareString);
            return IsOutOfDate(cacheKey, ticksToCompare, timeStore);
        }

        private bool IsOutOfDate(string cacheKey, long ticksToCompare, ITimeStore timeStore)
        {

            //we get the 
            var bytes = timeStore.GetValueFromStore(cacheKey);
            if (bytes == null)
                throw new ApplicationException(
                    $"You must seed the database with a cache value for the key {cacheKey}.");

            var cachedTicks = BitConverter.ToInt64(bytes, 0);
            return ticksToCompare < cachedTicks;
        }

        public void AddOrUpdate(string cacheKey, long cachedValue, ITimeStore databaseAccess)
        {
            var bytes = BitConverter.GetBytes(cachedValue);
            databaseAccess.AddUpdateValue(cacheKey, bytes);
        }
    }
}
