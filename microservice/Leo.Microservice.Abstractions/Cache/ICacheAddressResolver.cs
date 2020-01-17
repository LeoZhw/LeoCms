using Leo.Microservice.Abstractions.Cache.HashAlgorithms;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.Abstractions.Cache
{
    public interface ICacheAddressResolver
    {
        ValueTask<EndPoint> Resolver(string cacheId, string item);
    }
}
