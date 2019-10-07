using Leo.Microservice.Abstractions.Route;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.Microservice.Abstractions.Cache
{
    public interface ICacheNodeProvider
    {
        IEnumerable<ServiceRoute> GetServiceCaches();
    }
}
