using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.Microservice.Utils.Serialization
{
    /// <summary>
    /// 基于string类型的byte[]序列化器。
    /// </summary>
    public class StringByteArraySerializer : ISerializer<byte[]>
    {
        #region Field

        private readonly ISerializer<string> _serializer;

        #endregion Field

        #region Constructor

        public StringByteArraySerializer(ISerializer<string> serializer)
        {
            _serializer = serializer;
        }

        #endregion Constructor

        #region Implementation of ISerializer<byte[]>

        /// <summary>
        /// 序列化。
        /// </summary>
        /// <param name="instance">需要序列化的对象。</param>
        /// <returns>序列化之后的结果。</returns>
        public byte[] Serialize(object instance)
        {
            return Encoding.UTF8.GetBytes(_serializer.Serialize(instance));
        }

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <param name="content">序列化的内容。</param>
        /// <param name="type">对象类型。</param>
        /// <returns>一个对象实例。</returns>
        public object Deserialize(byte[] content, Type type)
        {
            return _serializer.Deserialize(Encoding.UTF8.GetString(content), type);
        }

        /// <summary>
        /// 反序列化方法
        /// </summary>
        /// <typeparam name="TResult">返回对象类型。</typeparam>
        /// <param name="serializer"><see cref="ISerializer"/>对象</param>
        /// <param name="content">源对象</param>
        /// <returns>返回反序列化对象</returns>
        public TResult Deserialize<TResult>(byte[] content)
        {
            return (TResult)Deserialize(content, typeof(TResult));
        }

        #endregion Implementation of ISerializer<byte[]>
    }
}
