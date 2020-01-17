using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.Abstractions.Cache
{
    public interface IServiceCacheManager
    {
        event EventHandler<ServiceCacheEventArgs> Created;

        event EventHandler<ServiceCacheEventArgs> Removed;

        event EventHandler<ServiceCacheChangedEventArgs> Changed;

        Task<IEnumerable<ServiceCache>> GetCachesAsync();

        Task SetCachesAsync(IEnumerable<ServiceCache> caches);

        Task SetCachesAsync(IEnumerable<ServiceCacheDescriptor> cacheDescriptors);

        Task RemveAddressAsync(IEnumerable<EndPoint> endpoints);

        Task ClearAsync();
    }

    public class ServiceCacheEventArgs
    {
        public ServiceCacheEventArgs(ServiceCache cache)
        {
            Cache = cache;
        }

        public ServiceCache Cache { get; private set; }
    }

    public class ServiceCacheChangedEventArgs : ServiceCacheEventArgs
    {
        public ServiceCacheChangedEventArgs(ServiceCache cache, ServiceCache oldCache) : base(cache)
        {
            OldCache = oldCache;
        }

        public ServiceCache OldCache { get; set; }
    }
}
