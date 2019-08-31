using Autofac;
using Leo.Microservice.Abstractions.Transport;
using Leo.Microservice.Host;
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

        //public static IServiceHostBuilder UseClient(this IServiceHostBuilder hostBuilder)
        //{
        //    return hostBuilder.MapServices(mapper =>
        //    {
        //        var serviceEntryManager = mapper.Resolve<IServiceEntryManager>();
        //        var addressDescriptors = serviceEntryManager.GetEntries().Select(i =>
        //        {
        //            i.Descriptor.Metadatas = null;
        //            return new ServiceSubscriber
        //            {
        //                Address = new[] { new IpAddressModel {
        //                     Ip = Dns.GetHostEntry(Dns.GetHostName())
        //                     .AddressList.FirstOrDefault<IPAddress>
        //                     (a => a.AddressFamily.ToString().Equals("InterNetwork")).ToString() } },
        //                ServiceDescriptor = i.Descriptor
        //            };
        //        }).ToList();
        //        mapper.Resolve<IServiceSubscribeManager>().SetSubscribersAsync(addressDescriptors);
        //        mapper.Resolve<IModuleProvider>().Initialize();
        //    });
        //}

        //public static void BuildServiceEngine(IContainer container)
        //{
        //    if (container.IsRegistered<IServiceEngine>())
        //    {
        //        var builder = new ContainerBuilder();

        //        container.Resolve<IServiceEngineBuilder>().Build(builder);
        //        var configBuilder = container.Resolve<IConfigurationBuilder>();
        //        var appSettingPath = Path.Combine(AppConfig.ServerOptions.RootPath, "appsettings.json");
        //        configBuilder.AddCPlatformFile("${appsettingspath}|" + appSettingPath, optional: false, reloadOnChange: true);
        //        builder.Update(container);
        //    }
        //}

        //public static async Task ConfigureRoute(IContainer mapper)
        //{
        //    if (AppConfig.ServerOptions.Protocol == CommunicationProtocol.Tcp ||
        //     AppConfig.ServerOptions.Protocol == CommunicationProtocol.None)
        //    {
        //        var routeProvider = mapper.Resolve<IServiceRouteProvider>();
        //        if (AppConfig.ServerOptions.EnableRouteWatch)
        //            new ServiceRouteWatch(mapper.Resolve<CPlatformContainer>(),
        //                async () => await routeProvider.RegisterRoutes(
        //                Math.Round(Convert.ToDecimal(Process.GetCurrentProcess().TotalProcessorTime.TotalSeconds), 2, MidpointRounding.AwayFromZero)));
        //        else
        //            await routeProvider.RegisterRoutes(0);
        //    }
        //}
    }
}
