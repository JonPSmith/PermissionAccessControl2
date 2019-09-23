namespace RefreshClaimsParts
{
    /// <summary>
    /// Access to the TimeStore part of the ExtraAuthorizeDbContext
    /// </summary>
    public interface ITimeStore
    {
        /// <summary>
        /// This reads the TimeStore entry with the given key.
        /// </summary>
        /// <param name="key">the cache key</param>
        /// <returns>DateTime ticks value, or null if not set.</returns>
        long? GetValueFromStore(string key);

        /// <summary>
        /// This adds or updates the TimeStore entry defined by the key
        /// </summary>
        /// <param name="key">the cache key</param>
        /// <param name="ticks">the new DateTime ticks value</param>
        void AddUpdateValue(string key, long ticks);
    }
}