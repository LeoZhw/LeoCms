using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.Microservice.Host
{
    public interface IServiceHost : IDisposable
    {
        IDisposable Run();

        IContainer Initialize();
    }
}
