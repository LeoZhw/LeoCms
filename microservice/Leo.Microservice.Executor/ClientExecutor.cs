using Leo.Microservice.Abstractions.Executor;
using Leo.Microservice.Abstractions.Serialization;
using Leo.Microservice.Abstractions.Transport;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.Executor
{
    public class ClientExecutor : IServiceExecutor
    {
        private readonly ILogger<ClientExecutor> _logger;

        public ClientExecutor(ILogger<ClientExecutor> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// 执行。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="message">调用消息。</param>
        public async Task ExecuteAsync(IMessageSender sender, TransportMessage message)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"客户端接收到消息。");
            Console.WriteLine($"客户端接收到消息，ID:{ message.Id}");
            return;
        }
    }
}
