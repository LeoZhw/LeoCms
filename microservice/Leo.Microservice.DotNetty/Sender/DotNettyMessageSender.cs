using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Leo.Microservice.Abstractions.Serialization;
using Leo.Microservice.Abstractions.Transport;

namespace Leo.Microservice.DotNetty.Sender
{
    /// <summary>
    /// 基于DotNetty的消息发送者基类。
    /// </summary>
    public abstract class DotNettyMessageSender
    {
        private readonly ITransportMessageEncoder _transportMessageEncoder;

        protected DotNettyMessageSender(ITransportMessageEncoder transportMessageEncoder)
        {
            _transportMessageEncoder = transportMessageEncoder;
        }

        protected IByteBuffer GetByteBuffer(TransportMessage message)
        {
            var data = _transportMessageEncoder.Encode(message);
            //var buffer = PooledByteBufferAllocator.Default.Buffer();
            return Unpooled.WrappedBuffer(data);
        }
    }
}
