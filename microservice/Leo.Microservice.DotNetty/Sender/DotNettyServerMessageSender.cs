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
    /// 基于DotNetty服务端的消息发送者。
    /// </summary>
    public class DotNettyServerMessageSender : DotNettyMessageSender, IMessageSender
    {
        private readonly IChannelHandlerContext _context;

        public DotNettyServerMessageSender(ITransportMessageEncoder transportMessageEncoder, IChannelHandlerContext context) : base(transportMessageEncoder)
        {
            _context = context;
        }

        #region Implementation of IMessageSender

        /// <summary>
        /// 发送消息。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            await _context.WriteAsync(buffer);
        }

        /// <summary>
        /// 发送消息并清空缓冲区。
        /// </summary>
        /// <param name="message">消息内容。</param>
        /// <returns>一个任务。</returns>
        public async Task SendAndFlushAsync(TransportMessage message)
        {
            var buffer = GetByteBuffer(message);
            await _context.WriteAndFlushAsync(buffer);
        }

        #endregion Implementation of IMessageSender
    }
}
