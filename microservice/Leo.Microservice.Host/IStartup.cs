using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.Microservice.Host
{
    public interface IStartup
    {
        IContainer ConfigureServices(ContainerBuilder services);

        void Configure(IContainer app);

        //ASP.Net中原有的IStartup中用来依赖注入的方法
        //IServiceProvider ConfigureServices(IServiceCollection services);
        //void Configure(IApplicationBuilder app);

    }
}
