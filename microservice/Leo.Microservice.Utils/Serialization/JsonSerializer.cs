using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Leo.Microservice.Utils.Serialization
{
    /// <summary>
    /// Json序列化器。
    /// </summary>
    public sealed class JsonSerializer : ISerializer<string>
    {
        #region Implementation of ISerializer<string>

        /// <summary>
        /// 序列化。
        /// </summary>
        /// <param name="instance">需要序列化的对象。</param>
        /// <returns>序列化之后的结果。</returns>
        public string Serialize(object instance)
        {
            return JsonConvert.SerializeObject(instance);
        }

        /// <summary>
        /// 反序列化。
        /// </summary>
        /// <param name="content">序列化的内容。</param>
        /// <param name="type">对象类型。</param>
        /// <returns>一个对象实例。</returns>
        public object Deserialize(string content, Type type)
        {
            return JsonConvert.DeserializeObject(content, type);
        }

        /// <summary>
        /// 反序列化方法
        /// </summary>
        /// <typeparam name="TResult">返回对象类型。</typeparam>
        /// <param name="serializer"><see cref="ISerializer"/>对象</param>
        /// <param name="content">源对象</param>
        /// <returns>返回反序列化对象</returns>
        public TResult Deserialize<TResult>(string content)
        {
            return (TResult)Deserialize(content, typeof(TResult));
        }

        #endregion Implementation of ISerializer<string>
    }
}
