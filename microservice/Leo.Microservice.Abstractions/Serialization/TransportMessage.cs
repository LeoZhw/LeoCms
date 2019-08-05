using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.Microservice.Abstractions.Serialization
{
    public class TransportMessage
    {
        /// <summary>
        /// 消息Id。
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 消息内容。
        /// </summary>
        public object Content { get; set; }

        /// <summary>
        /// 内容类型。
        /// </summary>
        public string ContentType { get; set; }
    }
}
