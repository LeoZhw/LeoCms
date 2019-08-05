using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.Microservice.Abstractions.Serialization
{
    /// <summary>
    /// 解码器
    /// </summary>
    public interface ITransportMessageDecoder
    {
        TransportMessage Decode(byte[] data);
    }
}
