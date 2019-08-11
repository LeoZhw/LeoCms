using System;
using System.Collections.Generic;
using System.Text;
using Leo.Microservice.Abstractions.Serialization;
using MessagePack;

namespace Leo.Microservice.MessagePack
{
    public sealed class MessagePackTransportMessageEncoder : ITransportMessageEncoder
    {
        #region Implementation of ITransportMessageEncoder

        public byte[] Encode(TransportMessage transportMessage)
        {
            MessagePackTransportMessage messagePackTransportMessage = new MessagePackTransportMessage(transportMessage);
            return MessagePackSerializer.Serialize(messagePackTransportMessage);
        }
        
        #endregion Implementation of ITransportMessageEncoder
    }
}
