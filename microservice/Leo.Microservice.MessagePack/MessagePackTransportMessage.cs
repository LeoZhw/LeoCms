using System;
using System.Collections.Generic;
using System.Text;
using Leo.Microservice.Abstractions.Serialization;
using MessagePack;

namespace Leo.Microservice.MessagePack
{
    [MessagePackObject]
    public class MessagePackTransportMessage
    {
        private TransportMessage _transportMessage;
        public MessagePackTransportMessage(): this(new TransportMessage())
        {
        }

        public MessagePackTransportMessage(TransportMessage transportMessage)
        {
            this._transportMessage = transportMessage;
        }

        public TransportMessage GetTransportMessage()
        {
            return _transportMessage;
        }
        /// <summary>
        /// 消息Id。
        /// </summary>
        [Key(0)]
        public string Id
        {
            get { return _transportMessage.Id; }
            set { _transportMessage.Id = value; }
        }

        /// <summary>
        /// 消息内容。
        /// </summary>
        [Key(1)]
        public object Content
        {
            get { return _transportMessage.Content; }
            set { _transportMessage.Content = value; }
        }

        /// <summary>
        /// 内容类型。
        /// </summary>
        [Key(2)]
        public string ContentType
        {
            get { return _transportMessage.ContentType; }
            set { _transportMessage.ContentType = value; }
        }
    }
}
