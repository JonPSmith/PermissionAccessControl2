namespace RefreshClaimsParts
{
    public interface IAuthChanges
    {
        /// <summary>
        /// This returns true if there is no ticksToCompare or the ticksToCompare is earlier than the AuthLastUpdated time
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="ticksToCompareString"></param>
        /// <param name="timeStore">Link to the DbContext that managers the cache store</param>
        /// <returns></returns>
        bool IsOutOfDateOrMissing(string cacheKey, string ticksToCompareString, ITimeStore timeStore);

        /// <summary>
        /// This adds or updates the TimeStore entry with the cacheKey with the cachedValue (datetime as ticks) 
        /// </summary>
        /// <param name="timeStore"></param>
        void AddOrUpdate(ITimeStore timeStore);
    }
}