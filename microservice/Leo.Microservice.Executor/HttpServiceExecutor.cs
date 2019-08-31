using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Leo.Microservice.Abstractions.Executor;
using Leo.Microservice.Abstractions.Serialization;
using Leo.Microservice.Abstractions.Transport;
using Microsoft.Extensions.Logging;

namespace Leo.Microservice.Executor
{
    public class HttpServiceExecutor : IServiceExecutor
    {
        #region Field
        
        private readonly ILogger<HttpServiceExecutor> _logger;
        #endregion Field

        #region Constructor

        public HttpServiceExecutor(ILogger<HttpServiceExecutor> logger)
        {
            _logger = logger;
        }

        #endregion Constructor

        #region Implementation of IServiceExecutor

        /// <summary>
        /// 执行。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="message">调用消息。</param>
        public async Task ExecuteAsync(IMessageSender sender, TransportMessage message)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace($"服务提供者接收到消。");
            Console.WriteLine($"服务提供者接收到消息，Content:{message.Content}，ID:{ message.Id}");
            return;

        }
        #endregion Implementation of IServiceExecutor
    }
}
