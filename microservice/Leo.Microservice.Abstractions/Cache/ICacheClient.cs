using Leo.Microservice.Abstractions.Cache.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.Abstractions.Cache
{
    public interface ICacheClient<T>
    {
        T GetClient(CacheEndpoint info, int connectTimeout);
        Task<bool> ConnectionAsync(CacheEndpoint endpoint, int connectTimeout);
    }
}
