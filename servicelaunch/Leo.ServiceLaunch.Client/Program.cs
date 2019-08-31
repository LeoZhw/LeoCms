using Autofac;
using Leo.Microservice.Abstractions.Executor;
using Leo.Microservice.Abstractions.Serialization;
using Leo.Microservice.Abstractions.Transport;
using Leo.Microservice.DotNetty;
using Leo.Microservice.Host;
using Leo.Microservice.MessagePack;
using Microsoft.Extensions.Logging;
using System;

namespace Leo.ServiceLaunch.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client, Hello World!");

            var host = new ServiceHostBuilder()
                .RegisterServices(builder =>
                {
                    builder.RegisterType<MessagePackTransportMessageCodecFactory>().As<ITransportMessageCodecFactory>().SingleInstance();
                    //builder.RegisterType(typeof(RemoteInvokeService)).As(typeof(IRemoteInvokeService)).SingleInstance();
                    //services.RegisterType<ServiceProxyGenerater>().As<IServiceProxyGenerater>().SingleInstance();
                    //services.RegisterType<ServiceProxyProvider>().As<IServiceProxyProvider>().SingleInstance();
                    builder.Register(provider =>
                    {
                        IServiceExecutor serviceExecutor = null;  
                        if (provider.IsRegistered(typeof(IServiceExecutor)))  // 没有注册客户端接收消息执行器，因此一直为空
                            serviceExecutor = provider.Resolve<IServiceExecutor>();
                        return new DotNettyTransportClientFactory(provider.Resolve<ITransportMessageCodecFactory>(),
                            provider.Resolve<ILogger<DotNettyTransportClientFactory>>(),
                            serviceExecutor);
                    }).As(typeof(ITransportClientFactory)).SingleInstance();
                })
                .UseStartup<Startup>()
                .Build();

            using (host.Run())
            {
                Startup.Test();
            }
            Console.ReadLine();
        }
    }
}
