using Leo.Microservice.Abstractions.Cache.HashAlgorithms;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.Abstractions.Cache
{
    public interface ICacheClient<T>
    {
        T GetClient(EndPoint info, int connectTimeout);
        Task<bool> ConnectionAsync(EndPoint endpoint, int connectTimeout);
    }
}
