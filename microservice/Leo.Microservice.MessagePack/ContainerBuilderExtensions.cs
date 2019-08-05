using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Leo.Microservice.Abstractions.Serialization;

namespace Leo.Microservice.MessagePack
{
    public static class ContainerBuilderExtensions
    {
        /// <summary>
        /// 使用messagepack编码解码方式
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ContainerBuilder UseMessagePackCodec(this ContainerBuilder builder)
        {
            builder.RegisterType(typeof(MessagePackTransportMessageCodecFactory)).As(typeof(ITransportMessageCodecFactory)).SingleInstance();
            return builder;
        }
    }
}
