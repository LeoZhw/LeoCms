using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.Zookeeper.WatcherProvider
{
    internal class ReconnectionWatcher : Watcher
    {
        private readonly Action _reconnection;

        public ReconnectionWatcher(Action reconnection)
        {
            _reconnection = reconnection;
        }

        #region Overrides of Watcher

        /// <summary>Processes the specified event.</summary>
        /// <param name="watchedEvent">The event.</param>
        /// <returns></returns>
        public override async Task process(WatchedEvent watchedEvent)
        {
            var state = watchedEvent.getState();
            switch (state)
            {
                case Event.KeeperState.Expired:
                case Event.KeeperState.Disconnected:
                    {
                        _reconnection();
                        break;
                    }
            }
            await Task.CompletedTask;
        }

        #endregion Overrides of Watcher
    }
}
