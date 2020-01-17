using Autofac;
using Leo.Microservice.Abstractions.Cache;
using Leo.Microservice.Abstractions.Cache.HashAlgorithms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Leo.Microservice.Redis
{
    /// <summary>
    /// 容器生成扩展 
    /// </summary>
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// 附加缓存注入 
        /// </summary>
        /// <param name="builder">服务构建者</param>
        /// <returns>服务构建者</returns>
        public static ContainerBuilder AddCache(this ContainerBuilder builder)
        {
            builder.RegisterType(typeof(RedisAddressResolver)).As(typeof(ICacheAddressResolver)).SingleInstance();
            builder.RegisterType(typeof(HashAlgorithm)).As(typeof(IHashAlgorithm)).SingleInstance();
            return builder;
        }

    }
}
