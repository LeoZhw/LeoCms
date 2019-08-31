using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.Abstractions.Transport
{
    public interface ITransportHost : IDisposable
    {
        /// <summary>
        /// 启动主机。
        /// </summary>
        /// <param name="endPoint">主机终结点。</param>
        /// <returns>一个任务。</returns>
        Task StartAsync(EndPoint endPoint);

        /// <summary>
        /// 启动主机。
        /// </summary>
        /// <param name="endPoint">ip地址。</param>
        Task StartAsync(string ip, int port);
    }
}
