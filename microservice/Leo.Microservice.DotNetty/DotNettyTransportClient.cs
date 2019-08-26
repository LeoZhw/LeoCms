using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Leo.Microservice.Abstractions.Executor;
using Leo.Microservice.Abstractions.Serialization;
using Leo.Microservice.Abstractions.Transport;
using Microsoft.Extensions.Logging;

namespace Leo.Microservice.DotNetty
{
    /// <summary>
    /// 一个默认的传输客户端实现。
    /// </summary>
    public class DotNettyTransportClient : ITransportClient, IDisposable
    {
        #region Field

        private readonly IMessageSender _messageSender;
        private readonly IMessageListener _messageListener;
        private readonly ILogger _logger;
        private readonly IServiceExecutor _serviceExecutor;

        #endregion Field

        #region Constructor

        public DotNettyTransportClient(IMessageSender messageSender, IMessageListener messageListener, ILogger logger, IServiceExecutor serviceExecutor)
        {
            _messageSender = messageSender;
            _messageListener = messageListener;
            _logger = logger;
            _serviceExecutor = serviceExecutor;
            messageListener.Received += MessageListener_Received;
        }

        #endregion Constructor

        #region Implementation of ITransportClient

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">远程调用消息模型。</param>
        /// <returns>远程调用消息的传输消息。</returns>
        public async Task SendAsync(TransportMessage transportMessage)
        {
            try
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("准备发送消息。");

                try
                {
                    //发送
                    await _messageSender.SendAndFlushAsync(transportMessage);
                }
                catch (Exception exception)
                {
                    throw new Exception("与服务端通讯时发生了异常。", exception);
                }

                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug("消息发送成功。");
                
            }
            catch (Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Error))
                    _logger.LogError(null,exception, "消息发送失败。");
                throw;
            }
        }

        #endregion Implementation of ITransportClient

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            (_messageSender as IDisposable)?.Dispose();
            (_messageListener as IDisposable)?.Dispose();
        }

        #endregion Implementation of IDisposable

        #region Private Method

        private async Task MessageListener_Received(IMessageSender sender, TransportMessage message)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
                _logger.LogTrace("服务消费者接收到消息。");
            
            if (_serviceExecutor != null)
                await _serviceExecutor.ExecuteAsync(sender, message);
        }

        #endregion Private Method
    }
}
