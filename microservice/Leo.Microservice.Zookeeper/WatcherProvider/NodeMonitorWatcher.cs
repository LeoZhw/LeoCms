using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.Zookeeper.WatcherProvider
{
    internal class NodeMonitorWatcher : Watcher
    {
        private readonly Task<ZooKeeper> _zooKeeperCall;
        private readonly string _path;
        private readonly Action<byte[], byte[]> _action;
        private byte[] _currentData;

        public NodeMonitorWatcher(Task<ZooKeeper> zooKeeperCall, string path, Action<byte[], byte[]> action)
        {
            _zooKeeperCall = zooKeeperCall;
            _path = path;
            _action = action;
        }

        public NodeMonitorWatcher SetCurrentData(byte[] currentData)
        {
            _currentData = currentData;

            return this;
        }

        #region Overrides of WatcherBase

        public override async Task process(WatchedEvent watchedEvent)
        {
            switch (watchedEvent.get_Type())
            {
                case Event.EventType.NodeDataChanged:
                    var zooKeeper = await _zooKeeperCall;
                    var watcher = new NodeMonitorWatcher(_zooKeeperCall, _path, _action);
                    var data = await zooKeeper.getDataAsync(_path, watcher);
                    var newData = data.Data;
                    _action(_currentData, newData);
                    watcher.SetCurrentData(newData);
                    break;
            }
        }

        #endregion Overrides of WatcherBase
    }
}
