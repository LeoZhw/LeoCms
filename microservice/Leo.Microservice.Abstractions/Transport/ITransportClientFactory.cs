using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.Abstractions.Transport
{
    /// <summary>
    /// 一个抽象的传输客户端工厂。
    /// </summary>
    public interface ITransportClientFactory
    {
        /// <summary>
        /// 创建客户端。
        /// </summary>
        /// <param name="endPoint">终结点。</param>
        /// <returns>传输客户端实例。</returns>
        Task<ITransportClient> CreateClientAsync(EndPoint endPoint);
    }
}
