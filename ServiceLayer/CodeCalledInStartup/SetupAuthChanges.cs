using System;
using CommonCache;
using DataLayer.EfCode;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceLayer.CodeCalledInStartup
{
    public static class SetupAuthChanges
    {
        public enum CacheTypes { InMemory, Redis}

        public static void ConfigureAuthChanges(this IServiceCollection services, CacheTypes cacheType)
        {
            switch (cacheType)
            {
                case CacheTypes.InMemory:
                    services.AddMemoryCache();
                    break;
                case CacheTypes.Redis:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(cacheType), cacheType, null);
            }

            var sp = services.BuildServiceProvider();
            var cache = sp.GetRequiredService<IDistributedCache>();
            var extraAuth = sp.GetRequiredService<ExtraAuthorizeDbContext>();

            services.AddSingleton<IAuthChanges>(AuthChanges.AuthChangesFactory(cache, extraAuth));
        }
    }
}