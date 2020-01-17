using Leo.Microservice.Abstractions.Cache;
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
    public class ZookeeperServiceCacheManager
    {
        private readonly ConfigInfo _configInfo;
        private readonly ISerializer<byte[]> _serializer;
        private readonly ILogger<ZookeeperServiceCacheManager> _logger;
        private ServiceCache[] _serviceCaches;
        private readonly ISerializer<string> _stringSerializer;
        private readonly ZookeeperClientProvider _zookeeperClientProvider;

        public ZookeeperServiceCacheManager(ConfigInfo configInfo, ISerializer<byte[]> serializer,
        ISerializer<string> stringSerializer,
        ILogger<ZookeeperServiceCacheManager> logger, ZookeeperClientProvider zookeeperClientProvider)
        {
            _configInfo = configInfo;
            _serializer = serializer;
            _stringSerializer = stringSerializer;
            _logger = logger;
            _zookeeperClientProvider = zookeeperClientProvider;
            EnterCaches().Wait();
        }

        private EventHandler<ServiceCacheEventArgs> _created;
        private EventHandler<ServiceCacheEventArgs> _removed;
        private EventHandler<ServiceCacheChangedEventArgs> _changed;

        public event EventHandler<ServiceCacheEventArgs> Created
        {
            add { _created += value; }
            remove { _created -= value; }
        }

        public event EventHandler<ServiceCacheEventArgs> Removed
        {
            add { _removed += value; }
            remove { _removed -= value; }
        }

        public event EventHandler<ServiceCacheChangedEventArgs> Changed
        {
            add { _changed += value; }
            remove { _changed -= value; }
        }

        protected void OnCreated(params ServiceCacheEventArgs[] args)
        {
            if (_created == null)
                return;

            foreach (var arg in args)
                _created(this, arg);
        }

        protected void OnChanged(params ServiceCacheChangedEventArgs[] args)
        {
            if (_changed == null)
                return;

            foreach (var arg in args)
                _changed(this, arg);
        }

        protected void OnRemoved(params ServiceCacheEventArgs[] args)
        {
            if (_removed == null)
                return;

            foreach (var arg in args)
                _removed(this, arg);
        }

        public async Task ClearAsync()
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备清空所有缓存配置。");
            var zooKeepers = await _zookeeperClientProvider.GetZooKeepers();
            foreach (var zooKeeper in zooKeepers)
            {
                var path = _configInfo.CachePath;
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
                    _logger.LogInformation("服务缓存配置清空完成。");
            }
        }

        public void Dispose()
        {
        }

        public async Task SetCachesAsync(IEnumerable<ServiceCache> caches)
        {
            var serviceCaches = await GetCaches(caches.Select(p => p.CacheDescriptor.Id));
            await RemoveCachesAsync(caches);
            if (caches == null)
                throw new ArgumentNullException(nameof(caches));

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("准备添加服务命令。");
            var path = _configInfo.CachePath;
            var zooKeepers = await _zookeeperClientProvider.GetZooKeepers();
            foreach (var zooKeeper in zooKeepers)
            {
                await CreateSubdirectory(zooKeeper, _configInfo.CachePath);
                if (!path.EndsWith("/"))
                    path += "/";

                caches = caches.ToArray();

                foreach (var serviceCache in caches)
                {
                    var nodePath = $"{path}{serviceCache.CacheDescriptor.Id}";
                    var nodeData = _serializer.Serialize(serviceCache);
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
                    _logger.LogInformation("服务缓存添加成功。");
            }
        }

        public async Task<IEnumerable<ServiceCache>> GetCachesAsync()
        {
            await EnterCaches();
            return _serviceCaches;
        }

        public async Task RemveAddressAsync(IEnumerable<EndPoint> endpoints)
        {
            var caches = await GetCachesAsync();
            try
            {
                foreach (var cache in caches)
                {
                    cache.CacheEndpoint = cache.CacheEndpoint.Except(endpoints);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            await SetCachesAsync(caches);
        }

        private async Task<ServiceCache[]> GetCaches(IEnumerable<string> childrens)
        {
            var rootPath = _configInfo.CachePath;
            if (!rootPath.EndsWith("/"))
                rootPath += "/";

            childrens = childrens.ToArray();
            var caches = new List<ServiceCache>(childrens.Count());

            foreach (var children in childrens)
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                    _logger.LogDebug($"准备从节点：{children}中获取缓存信息。");

                var nodePath = $"{rootPath}{children}";
                var cache = await GetCache(nodePath);
                if (cache != null)
                    caches.Add(cache);
            }
            return caches.ToArray();
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

        private async Task<ServiceCache> GetCache(string path)
        {
            ServiceCache result = null;
            var zooKeeper = await GetZooKeeper();
            var watcher = new NodeMonitorWatcher(GetZooKeeper(), path,
                 async (oldData, newData) => await NodeChange(oldData, newData));
            if (await zooKeeper.existsAsync(path) != null)
            {
                var data = (await zooKeeper.getDataAsync(path, watcher)).Data;
                watcher.SetCurrentData(data);
                result = await GetCache(data);
            }
            return result;
        }

        #region 私有方法
        private async Task RemoveCachesAsync(IEnumerable<ServiceCache> caches)
        {
            var path = _configInfo.CachePath;
            if (!path.EndsWith("/"))
                path += "/";
            caches = caches.ToArray();
            if (_serviceCaches != null)
            {
                var zooKeepers = await _zookeeperClientProvider.GetZooKeepers();
                foreach (var zooKeeper in zooKeepers)
                {

                    var deletedCacheIds = caches.Select(i => i.CacheDescriptor.Id).ToArray();
                    foreach (var deletedCacheId in deletedCacheIds)
                    {
                        var nodePath = $"{path}{deletedCacheId}";
                        await zooKeeper.deleteAsync(nodePath);
                    }
                }
            }
        }

        private async Task EnterCaches()
        {
            if (_serviceCaches != null)
                return;
            var zooKeeper = await GetZooKeeper();
            var path = _configInfo.CachePath;
            var watcher = new ChildrenMonitorWatcher(GetZooKeeper(), path,
                async (oldChildrens, newChildrens) => await ChildrenChange(oldChildrens, newChildrens));
            if (await zooKeeper.existsAsync(path, watcher) != null)
            {
                var result = await zooKeeper.getChildrenAsync(path, watcher);
                var childrens = result.Children.ToArray();
                watcher.SetCurrentData(childrens);
                _serviceCaches = await GetCaches(childrens);
            }
            else
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"无法获取服务缓存信息，因为节点：{path}，不存在。");
                _serviceCaches = new ServiceCache[0];
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

        private async Task<ServiceCache> GetCache(byte[] data)
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"准备转换服务缓存，配置内容：{Encoding.UTF8.GetString(data)}。");

            if (data == null)
                return null;

            return await Task.Run(() =>
            {
                return _serializer.Deserialize<ServiceCache>(data);
            });
        }

        private async Task<ServiceCache> GetCacheData(string data)
        {
            if (data == null)
                return null;

            return await Task.Run(() =>
            {
                return _stringSerializer.Deserialize<ServiceCache>(data);
            });
        }

        private async Task<ServiceCache[]> GetCacheDatas(string[] caches)
        {
            List<ServiceCache> serviceCaches = new List<ServiceCache>();
            foreach (var cache in caches)
            {
                var serviceCache = await GetCacheData(cache);
                serviceCaches.Add(serviceCache);
            }
            return serviceCaches.ToArray();
        }

        private async Task<string[]> ConvertPaths(string[] datas)
        {
            List<string> paths = new List<string>();
            foreach (var data in datas)
            {
                var result = await GetCacheData(data);
                var serviceId = result?.CacheDescriptor.Id;
                if (!string.IsNullOrEmpty(serviceId))
                    paths.Add(serviceId);
            }
            return paths.ToArray();
        }

        private async Task NodeChange(byte[] oldData, byte[] newData)
        {
            if (DataEquals(oldData, newData))
                return;

            var newCache = await GetCache(newData);
            //得到旧的缓存。
            var oldCache = _serviceCaches.FirstOrDefault(i => i.CacheDescriptor.Id == newCache.CacheDescriptor.Id);

            lock (_serviceCaches)
            {
                //删除旧缓存，并添加上新的缓存。
                _serviceCaches =
                    _serviceCaches
                        .Where(i => i.CacheDescriptor.Id != newCache.CacheDescriptor.Id)
                        .Concat(new[] { newCache }).ToArray();
            }

            //触发缓存变更事件。
            OnChanged(new ServiceCacheChangedEventArgs(newCache, oldCache));
        }

        private async Task ChildrenChange(string[] oldChildrens, string[] newChildrens)
        {
            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"最新的节点信息：{string.Join(",", newChildrens)}");

            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                _logger.LogDebug($"旧的节点信息：{string.Join(",", oldChildrens)}");

            //计算出已被删除的节点。
            var deletedChildrens = oldChildrens.Except(newChildrens).ToArray();
            //计算出新增的节点。
            var createdChildrens = newChildrens.Except(oldChildrens).ToArray();

            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"需要被删除的服务缓存节点：{string.Join(",", deletedChildrens)}");
            if (_logger.IsEnabled(LogLevel.Debug))
                _logger.LogDebug($"需要被添加的服务缓存节点：{string.Join(",", createdChildrens)}");

            //获取新增的缓存信息。
            var newCaches = (await GetCaches(createdChildrens)).ToArray();

            var caches = _serviceCaches.ToArray();
            lock (_serviceCaches)
            {
                _serviceCaches = _serviceCaches
                    //删除无效的缓存节点。
                    .Where(i => !deletedChildrens.Contains(i.CacheDescriptor.Id))
                    //连接上新的缓存。
                    .Concat(newCaches)
                    .ToArray();
            }
            //需要删除的缓存集合。
            var deletedCaches = caches.Where(i => deletedChildrens.Contains(i.CacheDescriptor.Id)).ToArray();
            //触发删除事件。
            OnRemoved(deletedCaches.Select(cache => new ServiceCacheEventArgs(cache)).ToArray());

            //触发缓存被创建事件。
            OnCreated(newCaches.Select(cache => new ServiceCacheEventArgs(cache)).ToArray());

            if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Information))
                _logger.LogInformation("缓存数据更新成功。");
        }

        private async Task<ZooKeeper> GetZooKeeper()
        {
            var zooKeeper = await _zookeeperClientProvider.GetZooKeeper();
            return zooKeeper;
        }

        #endregion
    }
}
