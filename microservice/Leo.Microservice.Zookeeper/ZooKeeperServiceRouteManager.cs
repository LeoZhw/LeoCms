﻿using Leo.Microservice.Abstractions.Route;
using Leo.Microservice.Utils;
using Leo.Microservice.Utils.Serialization;
using Leo.Microservice.Zookeeper.WatcherProvider;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.Zookeeper
{
    public class ZooKeeperServiceRouteManager : IServiceRouteManager, IDisposable
    {
        private readonly ConfigInfo _configInfo;
        private readonly ISerializer<byte[]> _serializer;
        private readonly ILogger<ZooKeeperServiceRouteManager> _logger;
        private ServiceRoute[] _routes;
        private readonly ZookeeperClientProvider _zookeeperClientProvider;

        public ZooKeeperServiceRouteManager(ConfigInfo configInfo, ISerializer<byte[]> serializer,
            ILogger<ZooKeeperServiceRouteManager> logger,
            ZookeeperClientProvider zookeeperClientProvider)
        {
            _configInfo = configInfo;
            _serializer = serializer;
            _logger = logger;
            _zookeeperClientProvider = zookeeperClientProvider;
            EnterRoutes().Wait();
        }

        private EventHandler<ServiceRouteEventArgs> _created;
        private EventHandler<ServiceRouteEventArgs> _removed;
        private EventHandler<ServiceRouteChangedEventArgs> _changed;

        /// <summary>
        /// 服务路由被创建。
        /// </summary>
        public event EventHandler<ServiceRouteEventArgs> Created
        {
            add { _created += value; }
            remove { _created -= value; }
        }

        /// <summary>
        /// 服务路由被删除。
        /// </summary>
        public event EventHandler<ServiceRouteEventArgs> Removed
        {
            add { _removed += value; }
            remove { _removed -= value; }
        }

        /// <summary>
        /// 服务路由被修改。
        /// </summary>
        public event EventHandler<ServiceRouteChangedEventArgs> Changed
        {
            add { _changed += value; }
            remove { _changed -= value; }
        }



        protected void OnCreated(params ServiceRouteEventArgs[] args)
        {
            if (_created == null)
                return;

            foreach (var arg in args)
                _created(this, arg);
        }

        protected void OnChanged(params ServiceRouteChangedEventArgs[] args)
        {
            if (_changed == null)
                return;

            foreach (var arg in args)
                _changed(this, arg);
        }

        protected void OnRemoved(params ServiceRouteEventArgs[] args)
        {
            if (_removed == null)
                return;

            foreach (var arg in args)
                _removed(this, arg);
        }


        /// <summary>
        /// 获取所有可用的服务路由信息。
        /// </summary>
        /// <returns>服务路由集合。</returns>
        public async Task<IEnumerable<ServiceRoute>> GetRoutesAsync()
        {
            await EnterRoutes();
            return _routes;
        }

        /// <summary>
        /// 清空所有的服务路由。
        /// </summary>
        /// <returns>一个任务。</returns>
        public async Task ClearAsync()
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备清空所有路由配置。");
            var zooKeepers = await _zookeeperClientProvider.GetZooKeepers();
            foreach (var zooKeeper in zooKeepers)
            {
                var path = _configInfo.RoutePath;
                var childrens = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                var index = 0;
                while (childrens.Count() > 1)
                {
                    var nodePath = "/" + string.Join("/", childrens);

                    if (await zooKeeper.existsAsync(nodePath) != null)
                    {
                        var result = await zooKeeper.getChildrenAsync(nodePath);
                        if (result?.Children != null)
                        {
                            foreach (var child in result.Children)
                            {
                                var childPath = $"{nodePath}/{child}";
                                if (_logger.IsEnabled(LogLevel.Debug))
                                    _logger.LogDebug($"准备删除：{childPath}。");
                                await zooKeeper.deleteAsync(childPath);
                            }
                        }
                        if (_logger.IsEnabled(LogLevel.Debug))
                            _logger.LogDebug($"准备删除：{nodePath}。");
                        await zooKeeper.deleteAsync(nodePath);
                    }
                    index++;
                    childrens = childrens.Take(childrens.Length - index).ToArray();
                }
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("路由配置清空完成。");
            }
        }

        /// <summary>
        /// 设置服务路由。
        /// </summary>
        /// <param name="routes">服务路由集合。</param>
        /// <returns>一个任务。</returns>
        public async Task SetRoutesAsync(IEnumerable<ServiceRoute> routes)
        {
            var hostAddr = NetUtils.GetHostAddress();
            var serviceRoutes = await GetRoutes(routes.Select(p => p.ServiceRouteDescriptor.Id));
            if (serviceRoutes.Count() > 0)
            {
                foreach (var route in routes)
                {
                    var serviceRoute = serviceRoutes.Where(p => p.ServiceRouteDescriptor.Id == route.ServiceRouteDescriptor.Id).FirstOrDefault();
                    if (serviceRoute != null)
                    {
                        var addresses = serviceRoute.Address.Concat(
                          route.Address.Except(serviceRoute.Address)).ToList();

                        foreach (var address in route.Address)
                        {
                            addresses.Remove(addresses.Where(p => p.ToString() == address.ToString()).FirstOrDefault());
                            addresses.Add(address);
                        }
                        route.Address = addresses;
                    }
                }
            }
            //删除主机预留端口，用于监听服务，不是服务提供端口，因此不需要注册到注册中心
            await RemoveExceptRoutesAsync(routes, hostAddr);

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备添加服务路由。");
            var zooKeepers = await _zookeeperClientProvider.GetZooKeepers();
            foreach (var zooKeeper in zooKeepers)
            {
                await CreateSubdirectory(zooKeeper, _configInfo.RoutePath);

                var path = _configInfo.RoutePath;
                if (!path.EndsWith("/"))
                    path += "/";

                routes = routes.ToArray();

                foreach (var serviceRoute in routes)
                {
                    var nodePath = $"{path}{serviceRoute.ServiceRouteDescriptor.Id}";
                    var nodeData = _serializer.Serialize(serviceRoute);
                    if (await zooKeeper.existsAsync(nodePath) == null)
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                            _logger.LogDebug($"节点：{nodePath}不存在将进行创建。");

                        await zooKeeper.createAsync(nodePath, nodeData, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                    }
                    else
                    {
                        if (_logger.IsEnabled(LogLevel.Debug))
                            _logger.LogDebug($"将更新节点：{nodePath}的数据。");

                        var onlineData = (await zooKeeper.getDataAsync(nodePath)).Data;
                        if (!DataEquals(nodeData, onlineData))
                            await zooKeeper.setDataAsync(nodePath, nodeData);
                    }
                }
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("服务路由添加成功。");
            }
        }

        public async Task RemveAddressAsync(IEnumerable<EndPoint> Address)
        {
            var routes = await GetRoutesAsync();
            foreach (var route in routes)
            {
                route.Address = route.Address.Except(Address);
            }
            await SetRoutesAsync(routes);
        }

        private async Task RemoveExceptRoutesAsync(IEnumerable<ServiceRoute> routes, EndPoint hostAddr)
        {
            var path = _configInfo.RoutePath;
            if (!path.EndsWith("/"))
                path += "/";
            routes = routes.ToArray();
            var zooKeepers = await _zookeeperClientProvider.GetZooKeepers();
            foreach (var zooKeeper in zooKeepers)
            {
                if (_routes != null)
                {
                    var oldRouteIds = _routes.Select(i => i.ServiceRouteDescriptor.Id).ToArray();
                    var newRouteIds = routes.Select(i => i.ServiceRouteDescriptor.Id).ToArray();
                    var deletedRouteIds = oldRouteIds.Except(newRouteIds).ToArray();
                    foreach (var deletedRouteId in deletedRouteIds)
                    {
                        var addresses = _routes.Where(p => p.ServiceRouteDescriptor.Id == deletedRouteId).Select(p => p.Address).FirstOrDefault();
                        if (addresses.Contains(hostAddr))
                        {
                            var nodePath = $"{path}{deletedRouteId}";
                            await zooKeeper.deleteAsync(nodePath);
                        }
                    }
                }
            }
        }

        private async Task CreateSubdirectory(ZooKeeper zooKeeper, string path)
        {
            if (await zooKeeper.existsAsync(path) != null)
                return;

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"节点{path}不存在，将进行创建。");

            var childrens = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var nodePath = "/";

            foreach (var children in childrens)
            {
                nodePath += children;
                if (await zooKeeper.existsAsync(nodePath) == null)
                {
                    await zooKeeper.createAsync(nodePath, null, ZooDefs.Ids.OPEN_ACL_UNSAFE, CreateMode.PERSISTENT);
                }
                nodePath += "/";
            }
        }

        private async Task<ServiceRoute> GetRoute(byte[] data)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"准备转换服务路由，配置内容：{Encoding.UTF8.GetString(data)}。");

            if (data == null)
                return null;

            return await Task.Run(() =>
            {
                return _serializer.Deserialize<ServiceRoute>(data);
            });
        }

        private async Task<ServiceRoute> GetRoute(string path)
        {
            ServiceRoute result = null;
            var zooKeeper = await GetZooKeeper();
            var watcher = new NodeMonitorWatcher(GetZooKeeper(), path,
                 async (oldData, newData) => await NodeChange(oldData, newData));
            if (await zooKeeper.existsAsync(path) != null)
            {
                var data = (await zooKeeper.getDataAsync(path, watcher)).Data;
                watcher.SetCurrentData(data);
                result = await GetRoute(data);
            }
            return result;
        }

        private async Task<ServiceRoute[]> GetRoutes(IEnumerable<string> childrens)
        {
            var rootPath = _configInfo.RoutePath;
            if (!rootPath.EndsWith("/"))
                rootPath += "/";

            childrens = childrens.ToArray();
            var routes = new List<ServiceRoute>(childrens.Count());

            foreach (var children in childrens)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"准备从节点：{children}中获取路由信息。");

                var nodePath = $"{rootPath}{children}";
                var route = await GetRoute(nodePath);
                if (route != null)
                    routes.Add(route);
            }

            return routes.ToArray();
        }

        private async Task EnterRoutes()
        {
            if (_routes != null)
                return;
            var zooKeeper = await GetZooKeeper();
            var watcher = new ChildrenMonitorWatcher(GetZooKeeper(), _configInfo.RoutePath,
             async (oldChildrens, newChildrens) => await ChildrenChange(oldChildrens, newChildrens));
            if (await zooKeeper.existsAsync(_configInfo.RoutePath, watcher) != null)
            {
                var result = await zooKeeper.getChildrenAsync(_configInfo.RoutePath, watcher);
                var childrens = result.Children.ToArray();
                watcher.SetCurrentData(childrens);
                _routes = await GetRoutes(childrens);
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"无法获取路由信息，因为节点：{_configInfo.RoutePath}，不存在。");
                _routes = new ServiceRoute[0];
            }
        }

        private static bool DataEquals(IReadOnlyList<byte> data1, IReadOnlyList<byte> data2)
        {
            if (data1.Count != data2.Count)
                return false;
            for (var i = 0; i < data1.Count; i++)
            {
                var b1 = data1[i];
                var b2 = data2[i];
                if (b1 != b2)
                    return false;
            }
            return true;
        }

        public async Task NodeChange(byte[] oldData, byte[] newData)
        {
            if (DataEquals(oldData, newData))
                return;

            var newRoute = await GetRoute(newData);
            //得到旧的路由。
            var oldRoute = _routes.FirstOrDefault(i => i.ServiceRouteDescriptor.Id == newRoute.ServiceRouteDescriptor.Id);

            lock (_routes)
            {
                //删除旧路由，并添加上新的路由。
                _routes =
                    _routes
                        .Where(i => i.ServiceRouteDescriptor.Id != newRoute.ServiceRouteDescriptor.Id)
                        .Concat(new[] { newRoute }).ToArray();
            }

            //触发路由变更事件。
            OnChanged(new ServiceRouteChangedEventArgs(newRoute, oldRoute));
        }

        public async Task ChildrenChange(string[] oldChildrens, string[] newChildrens)
        {
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"最新的节点信息：{string.Join(",", newChildrens)}");

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"旧的节点信息：{string.Join(",", oldChildrens)}");

            //计算出已被删除的节点。
            var deletedChildrens = oldChildrens.Except(newChildrens).ToArray();
            //计算出新增的节点。
            var createdChildrens = newChildrens.Except(oldChildrens).ToArray();

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"需要被删除的路由节点：{string.Join(",", deletedChildrens)}");
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"需要被添加的路由节点：{string.Join(",", createdChildrens)}");

            //获取新增的路由信息。
            var newRoutes = (await GetRoutes(createdChildrens)).ToArray();

            var routes = _routes.ToArray();
            lock (_routes)
            {
                _routes = _routes
                    //删除无效的节点路由。
                    .Where(i => !deletedChildrens.Contains(i.ServiceRouteDescriptor.Id))
                    //连接上新的路由。
                    .Concat(newRoutes)
                    .ToArray();
            }
            //需要删除的路由集合。
            var deletedRoutes = routes.Where(i => deletedChildrens.Contains(i.ServiceRouteDescriptor.Id)).ToArray();
            //触发删除事件。
            OnRemoved(deletedRoutes.Select(route => new ServiceRouteEventArgs(route)).ToArray());

            //触发路由被创建事件。
            OnCreated(newRoutes.Select(route => new ServiceRouteEventArgs(route)).ToArray());

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("路由数据更新成功。");
        }


        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
        }

        private async Task<ZooKeeper> GetZooKeeper()
        {
            return await _zookeeperClientProvider.GetZooKeeper();
        }

    }
}
