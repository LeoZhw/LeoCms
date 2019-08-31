using Leo.Microservice.Abstractions.Executor;
using Leo.Microservice.Abstractions.Serialization;
using Leo.Microservice.Abstractions.Transport;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.DotNetty
{
    public class DotNettyTransportHost : ITransportHost
    {
        #region Field

        private IServiceExecutor _serviceExecutor;
        public IServiceExecutor ServiceExecutor { get => _serviceExecutor; }
        private readonly Func<EndPoint, Task<IMessageListener>> _messageListenerFactory;
        private IMessageListener _serverMessageListener;

        #endregion Field

        public DotNettyTransportHost(Func<EndPoint, Task<IMessageListener>> messageListenerFactory, IServiceExecutor serviceExecutor)
        {
            _messageListenerFactory = messageListenerFactory;
            _serviceExecutor = serviceExecutor;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            (_serverMessageListener as IDisposable)?.Dispose();
        }

        /// <summary>
        /// 启动主机。
        /// </summary>
        /// <param name="endPoint">主机终结点。</param>
        /// <returns>一个任务。</returns>
        public async Task StartAsync(EndPoint endPoint)
        {
            if (_serverMessageListener != null)
                return;
            _serverMessageListener = await _messageListenerFactory(endPoint);
            _serverMessageListener.Received += MessageListener_Received;
        }

        public async Task StartAsync(string ip, int port)
        {
            if (_serverMessageListener != null)
                return;
            _serverMessageListener = await _messageListenerFactory(new IPEndPoint(IPAddress.Parse(ip), port));
            _serverMessageListener.Received += MessageListener_Received;
            //await StartAsync(new IPEndPoint(IPAddress.Parse(ip), port));
        }

        /// <summary>
        /// 监听并回调
        /// </summary>
        /// <param name="sender">消息发送器</param>
        /// <param name="message">监听到的消息</param>
        /// <returns></returns>
        private async Task MessageListener_Received(IMessageSender sender, TransportMessage message)
        {
            await _serviceExecutor.ExecuteAsync(sender, message);
        }
    }
}
