using Autofac;
using Leo.Microservice.Abstractions.Transport;
using Leo.Microservice.Host;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.Extensions.ServiceHostBuilder
{
    public static class ServerExtensions
    {
        public static IServiceHostBuilder UseServer(this IServiceHostBuilder hostBuilder)
        {
            return hostBuilder.MapServices(async mapper =>
            {
                int _port = 981;
                string _ip = "127.0.0.1";

                Console.WriteLine($"准备启动服务主机，监听地址：{_ip}:{_port}。");
                var transportHosts = mapper.Resolve<IList<ITransportHost>>();
                Task.Factory.StartNew(async () =>
                {
                    foreach (var transportHost in transportHosts)
                        await transportHost.StartAsync(_ip, _port);
                }).Wait();
            });
        }

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
    }
}
