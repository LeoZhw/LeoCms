using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Leo.Microservice.Abstractions.Serialization;

namespace Leo.Microservice.Abstractions.Transport
{
    /// <summary>
    /// 一个抽象的传输客户端。
    /// </summary>
    public interface ITransportClient
    {
        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">远程调用消息模型。</param>
        /// <returns>远程调用消息的传输消息。</returns>
        Task SendAsync(TransportMessage transportMessage);
    }
}
