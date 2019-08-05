using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.Microservice.Abstractions.Serialization
{
    /// <summary>
    /// 编码器
    /// </summary>
    public interface ITransportMessageEncoder
    {
        byte[] Encode(TransportMessage message);
    }
}
