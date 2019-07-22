using System;

namespace CommonCache
{
    public interface IAuthChanges
    {
        /// <summary>
        /// This returns true if there is an entry in the cache and the ticks given are lower, i.e. we need to recalc things
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="ticksToCompareString"></param>
        /// <param name="getTimeStore">This func gets the timeStore - Func used to allow lazy creation of DbContext</param>
        /// <returns></returns>
        bool IsLowerThan(string cacheKey, string ticksToCompareString, Func<ITimeStore> getTimeStore);

        Action AddOrUpdate(string cacheKey, long cachedValue, ITimeStore databaseAccess);
    }
}