using System;

namespace CommonCache
{
    public interface IAuthChanges
    {
        /// <summary>
        /// This returns true if there is no ticksToCompare or the ticksToCompare is earlier than the AuthLastUpdated time
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="ticksToCompareString"></param>
        /// <param name="getTimeStore">This func gets the timeStore - Func used to allow lazy creation of DbContext</param>
        /// <returns></returns>
        bool IsOutOfDateOrMissing(string cacheKey, string ticksToCompareString, Func<ITimeStore> getTimeStore);

        Action AddOrUpdate(string cacheKey, long cachedValue, ITimeStore databaseAccess);
    }
}