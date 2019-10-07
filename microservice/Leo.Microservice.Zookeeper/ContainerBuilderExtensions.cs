using Autofac;
using Leo.Microservice.Utils.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.Microservice.Zookeeper
{
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// 设置共享文件路由管理者。
        /// </summary>
        /// <param name="builder">Rpc服务构建者。</param>
        /// <param name="configInfo">ZooKeeper设置信息。</param>
        /// <returns>服务构建者。</returns>
        public static ContainerBuilder UseZooKeeperRouteManager(this ContainerBuilder builder, ConfigInfo configInfo)
        {
            builder.RegisterAdapter(new Func<IServiceProvider, IServiceRouteManager>(provider =>
               new ZooKeeperServiceRouteManager(
               configInfo,
               provider.GetRequiredService<ISerializer<byte[]>>(),
               provider.GetRequiredService<ILogger<ZooKeeperServiceRouteManager>>(),
               provider.GetRequiredService<ZookeeperClientProvider>()))).InstancePerLifetimeScope();
            return builder;
        }

        public static ContainerBuilder UseZooKeeperManager(this ContainerBuilder builder, ConfigInfo configInfo)
        {
            return builder.UseZooKeeperRouteManager(configInfo)
                .UseZookeeperClientProvider(configInfo);
        }

        public static ContainerBuilder UseZooKeeperManager(this ContainerBuilder builder)
        {
            var configInfo = new ConfigInfo(null);
            return builder.UseZooKeeperRouteManager(configInfo)
                .UseZookeeperClientProvider(configInfo);
        }

        public static ContainerBuilder UseZookeeperClientProvider(this ContainerBuilder builder, ConfigInfo configInfo)
        {
            builder.Register(provider =>
            new ZookeeperClientProvider(
            configInfo,
            provider.Resolve<ILogger<ZookeeperClientProvider>>())).SingleInstance();
            return builder;
        }
    }
}
