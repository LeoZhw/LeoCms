using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Transport.Channels;
using Leo.Microservice.Abstractions.Serialization;
using Leo.Microservice.Abstractions.Transport;

namespace Leo.Microservice.DotNetty.Sender
{
    /// <summary>
    /// 基于DotNetty客户端的消息发送者。
    /// </summary>
    public class DotNettyMessageClientSender : DotNettyMessageSender, IMessageSender, IDisposable
    {
        private readonly IChannel _channel;

        public DotNettyMessageClientSender(ITransportMessageEncoder transportMessageEncoder, IChannel channel) : base(transportMessageEncoder)
        {
            _channel = channel;
        }

        #region Implementation of IDisposable

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            Task.Run(async () =>
            {
                await _channel.DisconnectAsync();
            }).Wait();
        }

        #endregion Implementation of IDisposable

        #region Implementation of IMessageSender

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            await _channel.WriteAndFlushAsync(buffer);
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            await _channel.WriteAndFlushAsync(buffer);
        }

        #endregion Implementation of IMessageSender
    }

}
