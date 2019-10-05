using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.Zookeeper.WatcherProvider
{
    internal class ChildrenMonitorWatcher : Watcher
    {
        private readonly Task<ZooKeeper> _zooKeeperCall;
        private readonly string _path;
        private readonly Action<string[], string[]> _action;
        private string[] _currentData = new string[0];

        public ChildrenMonitorWatcher(Task<ZooKeeper> zooKeeperCall, string path, Action<string[], string[]> action)
        {
            _zooKeeperCall = zooKeeperCall;
            _path = path;
            _action = action;
        }

        public ChildrenMonitorWatcher SetCurrentData(string[] currentData)
        {
            _currentData = currentData ?? new string[0];

            return this;
        }

        #region Overrides of WatcherBase

        public override async Task process(WatchedEvent watchedEvent)
        {
            if (watchedEvent.getState() != Event.KeeperState.SyncConnected || watchedEvent.getPath() != _path)
                return;
            var zooKeeper = await _zooKeeperCall;
            //Func<ChildrenMonitorWatcher> getWatcher = () => new ChildrenMonitorWatcher(_zooKeeperCall, path, _action);
            Task<ChildrenMonitorWatcher> getWatcher =  Task.Run(() => {return new ChildrenMonitorWatcher(_zooKeeperCall, _path, _action); });
            switch (watchedEvent.get_Type())
            {
                //创建之后开始监视下面的子节点情况。
                case Event.EventType.NodeCreated:
                    await zooKeeper.getChildrenAsync(_path, await getWatcher);
                    break;

                //子节点修改则继续监控子节点信息并通知客户端数据变更。
                case Event.EventType.NodeChildrenChanged:
                    try
                    {
                        var watcher = await getWatcher;
                        var result = await zooKeeper.getChildrenAsync(_path, watcher);
                        var childrens = result.Children.ToArray();
                        _action(_currentData, childrens);
                        watcher.SetCurrentData(childrens);
                    }
                    catch (KeeperException.NoNodeException)
                    {
                        _action(_currentData, new string[0]);
                    }
                    break;

                //删除之后开始监控自身节点，并通知客户端数据被清空。
                case Event.EventType.NodeDeleted:
                    {
                        var watcher = await getWatcher;
                        await zooKeeper.existsAsync(_path, watcher);
                        _action(_currentData, new string[0]);
                        watcher.SetCurrentData(new string[0]);
                    }
                    break;
            }
        }
        #endregion Overrides of WatcherBase
    }
}
