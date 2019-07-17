using Microsoft.Extensions.Caching.Distributed;

namespace CommonCache
{
    public class AuthChangesFactory : IAuthChangesFactory
    {
        private readonly IDistributedCache _cache;

        public AuthChangesFactory(IDistributedCache cache)
        {
            _cache = cache;
        }

        public IAuthChanges CreateIAuthChange(ITimeStore timeStore)
        {
            return new AuthChanges(_cache, timeStore);
        }
    }
}