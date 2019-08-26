using Leo.Microservice.Host;
using System;
using System.Text;

namespace Leo.ServiceLaunch.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Server, Hello World!");

            var host = new ServiceHostBuilder()
                //.RegisterServices(builder =>
                //{
                //})
                //.ConfigureLogging(logger =>
                //{
                //})
                //.Configure(builder =>
                //{
                //})
                //.Configure(builder =>
                //{
                //})
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
