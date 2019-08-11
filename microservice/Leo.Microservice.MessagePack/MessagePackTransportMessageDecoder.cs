using System;
using System.Collections.Generic;
using System.Text;
using Leo.Microservice.Abstractions.Serialization;
using MessagePack;

namespace Leo.Microservice.MessagePack
{
    public sealed class MessagePackTransportMessageDecoder : ITransportMessageDecoder
    {
        #region Implementation of ITransportMessageDecoder

        public TransportMessage Decode(byte[] data)
        {
            MessagePackTransportMessage messagePackTransportMessage = MessagePackSerializer.Deserialize<MessagePackTransportMessage>(data);
            return messagePackTransportMessage.GetTransportMessage();
        }

        #endregion Implementation of ITransportMessageDecoder
    }
}
