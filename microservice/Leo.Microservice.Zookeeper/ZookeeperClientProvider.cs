using Leo.Microservice.Zookeeper.WatcherProvider;
using Microsoft.Extensions.Logging;
using org.apache.zookeeper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.Zookeeper
{
    public class ZookeeperClientProvider
    {
        private ConfigInfo _config;
        private readonly ILogger<ZookeeperClientProvider> _logger;
        private readonly Dictionary<string, ZooKeeper> _zookeeperClients = new Dictionary<string, ZooKeeper>();

        public ZookeeperClientProvider(ConfigInfo config, ILogger<ZookeeperClientProvider> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<ZooKeeper> GetZooKeeper()
        {
            return await CreateZooKeeper(_config.Addresses.FirstOrDefault());
        }
        public async Task<ZooKeeper> CreateZooKeeper(string address)
        {
            if (!_zookeeperClients.TryGetValue(address, out ZooKeeper result))
            {
                await Task.Run(() =>
                {
                    result = new ZooKeeper(address, (int)_config.SessionTimeout.TotalMilliseconds,
                        new ReconnectionWatcher(
                            async () =>
                            {
                                if (_zookeeperClients.Remove(address, out ZooKeeper value))
                                {
                                    await value.closeAsync();
                                }
                                await CreateZooKeeper(address);
                            }));
                    _zookeeperClients.TryAdd(address, result);
                });
            }
            return result;
        }

        public async Task<IEnumerable<ZooKeeper>> GetZooKeepers()
        {
            var result = new List<ZooKeeper>();
            foreach (var address in _config.Addresses)
            {
                result.Add(await CreateZooKeeper(address));
            }
            return result;
        }
    }
}
