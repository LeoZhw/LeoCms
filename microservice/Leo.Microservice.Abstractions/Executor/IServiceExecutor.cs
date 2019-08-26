using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Leo.Microservice.Abstractions.Serialization;
using Leo.Microservice.Abstractions.Transport;

namespace Leo.Microservice.Abstractions.Executor
{
    /// <summary>
    /// 一个抽象的服务执行器。
    /// </summary>
    public interface IServiceExecutor
    {
        /// <summary>
        /// 执行。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="message">调用消息。</param>
        Task ExecuteAsync(IMessageSender sender, TransportMessage message);
    }
}
