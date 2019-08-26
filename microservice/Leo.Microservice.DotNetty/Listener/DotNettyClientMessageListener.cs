using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Leo.Microservice.Abstractions.Serialization;
using Leo.Microservice.Abstractions.Transport;

namespace Leo.Microservice.DotNetty.Listener
{
    /// <summary>
    /// 消息监听者。
    /// </summary>
    public class DotNettyClientMessageListener : IMessageListener
    {
        #region Implementation of IMessageListener

        /// <summary>
        /// 接收到消息的事件。
        /// </summary>
        public event ReceivedDelegate Received;

        /// <summary>
        /// 触发接收到消息事件。
        /// </summary>
        /// <param name="sender">消息发送者。</param>
        /// <param name="message">接收到的消息。</param>
        /// <returns>一个任务。</returns>
        public async Task OnReceived(IMessageSender sender, TransportMessage message)
        {
            if (Received == null)
                return;
            await Received(sender, message);
        }

        #endregion Implementation of IMessageListener
    }
}
