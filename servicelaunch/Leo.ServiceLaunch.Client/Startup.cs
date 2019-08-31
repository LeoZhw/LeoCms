using Autofac;
using Leo.Microservice.Abstractions.Serialization;
using Leo.Microservice.Abstractions.Transport;
using Leo.Microservice.Host;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Leo.ServiceLaunch.Client
{
    class Startup : IStartup
    {
        private static IContainer _container;
        public void Configure(IContainer app)
        {
        }

        public IContainer ConfigureServices(ContainerBuilder builder)
        {
            _container = builder.Build();
            return _container;
        }

        internal static void Test()
        {
            Task.Run(async () =>
            {
                do
                {
                    Console.WriteLine("正在循环 1万次发送消息.....");

                    //1w次调用
                    var watch = Stopwatch.StartNew();
                    for (var i = 1; i < 10000; i++)
                    {
                        var invokeMessage = new TransportMessage
                        {
                            Id = i.ToString(),
                            ContentType = "string",
                            Content = "你好啊，这是客户端发给服务端的消息"
                        };
                        try
                        {
                            var endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 981);
                            ITransportClientFactory transportClientFactory = _container.Resolve<ITransportClientFactory>();
                            var client = await transportClientFactory.CreateClientAsync(endPoint);
                            await client.SendAsync(invokeMessage);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception.ToString(), $"发起请求中发生了错误，服务Id：{invokeMessage.Id}。");
                            throw;
                        }
                    }
                    watch.Stop();
                    Console.WriteLine($"1万次发送结束，执行时间：{watch.ElapsedMilliseconds}ms");
                    Console.WriteLine("Press any key to continue, q to exit the loop...");
                    var key = Console.ReadLine();
                    if (key.ToLower() == "q")
                        break;
                } while (true);
            }).Wait();
        }
    }
}
