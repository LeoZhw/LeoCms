using Leo.Microservice.Host;
using Leo.Microservice.Abstractions.Executor;
using Leo.Microservice.Abstractions.Serialization;
using Leo.Microservice.Abstractions.Transport;
using Leo.Microservice.DotNetty;
using Leo.Microservice.DotNetty.Listener;
using System;
using System.Text;
using Autofac;
using Microsoft.Extensions.Logging;
using Leo.Microservice.Executor;
using Leo.Microservice.Extensions.ServiceHostBuilder;
using Leo.Microservice.MessagePack;

namespace Leo.ServiceLaunch.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server, Hello World!");

            var host = new ServiceHostBuilder()
                .RegisterServices(builder =>
                {
                    builder.RegisterType<MessagePackTransportMessageCodecFactory>().As<ITransportMessageCodecFactory>().SingleInstance();
                    builder.RegisterType(typeof(HttpServiceExecutor)).As(typeof(IServiceExecutor)).Named<IServiceExecutor>("tcp").SingleInstance();
                    builder.Register(provider =>
                    {
                        return new DotNettyServerMessageListener(provider.Resolve<ILogger<DotNettyServerMessageListener>>(),
                              provider.Resolve<ITransportMessageCodecFactory>());
                    }).SingleInstance();
                    builder.Register(provider =>
                    {
                        var serviceExecutor = provider.ResolveKeyed<IServiceExecutor>("tcp");
                        var messageListener = provider.Resolve<DotNettyServerMessageListener>();
                        return new DotNettyTransportHost(async endPoint =>
                        {
                            await messageListener.StartAsync(endPoint);
                            return messageListener;
                        }, serviceExecutor);
                    }).As<ITransportHost>();
                })
                .UseServer()  // 指定监听的端口
                .UseStartup<Startup>()
                .Build();

            using (host.Run())
            {
                Console.WriteLine($"服务端启动成功，{DateTime.Now}。");
            }

            Console.ReadLine();
        }
    }
}
