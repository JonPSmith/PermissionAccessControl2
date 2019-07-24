namespace CommonCache
{
    /// <summary>
    /// Access to the TimeStore.
    /// NOTE: I use byte[] to fit in with the IDistributedCache value definition - helps if you want to add IDistributedCache
    /// </summary>
    public interface ITimeStore
    {
        /// <summary>
        /// This reads the TimeStore entry with the given key.
        /// </summary>
        /// <param name="key">the cache key</param>
        /// <returns>byte[] holding the long time, or null if not set.</returns>
        byte[] GetValueFromStore(string key);

        /// <summary>
        /// This adds or updates the TimeStore entry defined by the key
        /// </summary>
        /// <param name="key">the cache key</param>
        /// <param name="value">byte[] holding the long time</param>
        void AddUpdateValue(string key, byte[] value);
    }
}