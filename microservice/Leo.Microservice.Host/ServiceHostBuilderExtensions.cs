using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Leo.Microservice.Host
{
    public static class ServiceHostBuilderExtensions
    {
        public static IServiceHostBuilder UseStartup(this IServiceHostBuilder hostBuilder, Type startupType)
        {
            return hostBuilder
                .ConfigureServices(services =>
                {
                    services.AddSingleton(typeof(IStartup), startupType);
                });
        }

        public static IServiceHostBuilder UseStartup<TStartup>(this IServiceHostBuilder hostBuilder) where TStartup : IStartup
        {
            return hostBuilder.UseStartup(typeof(TStartup));
        }

        //public static IServiceHostBuilder UseConsoleLifetime(this IServiceHostBuilder hostBuilder)
        //{
        //    return hostBuilder.ConfigureServices((collection) =>
        //    {
        //        collection.AddSingleton<IApplicationLifetime, ApplicationLifetime>();
        //        collection.AddSingleton<IHostLifetime, ConsoleLifetime>();
        //    });
        //}
    }
}
