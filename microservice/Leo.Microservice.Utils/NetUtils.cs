using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Leo.Microservice.Utils
{
    public class NetUtils
    {
        /// <summary>
        /// 获取主机的已经使用的端口，这些端口用于服务监听，不能配置到zookeeper节点中
        /// </summary>
        /// <returns></returns>
        public static EndPoint GetHostAddress()
        {
            return new IPEndPoint(IPAddress.Parse("127.0.0.1"),981);
        }
    }
}
