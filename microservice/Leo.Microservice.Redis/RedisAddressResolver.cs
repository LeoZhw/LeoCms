﻿using Autofac;
using Leo.Microservice.Abstractions.Cache;
using Leo.Microservice.Abstractions.Cache.HashAlgorithms;
using Leo.Microservice.Abstractions.Route;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.Redis
{
    public class RedisAddressResolver : ICacheAddressResolver
    {

        #region Field  
        private readonly ILogger<RedisAddressResolver> _logger;
        private readonly IServiceRouteManager _serviceRouteManager;
        private readonly IContainer _container;
        private readonly ConcurrentDictionary<string, ServiceRoute> _concurrent =
            new ConcurrentDictionary<string, ServiceRoute>();
        #endregion

        public RedisAddressResolver(ILogger<RedisAddressResolver> logger, IServiceRouteManager serviceRouteManager, IContainer container)
        {
            _logger = logger;
            _serviceRouteManager = serviceRouteManager;
            _container = container;
            _serviceRouteManager.Changed += ServiceRouteManager_Removed;
            _serviceRouteManager.Removed += ServiceRouteManager_Removed;
            _serviceRouteManager.Created += ServiceRouteManager_Add;
        }

        public async ValueTask<EndPoint> Resolver(string cacheId, string item)
        {

            _concurrent.TryGetValue(cacheId, out ServiceRoute descriptor);
            if (descriptor == null)
            {
                var descriptors = await _serviceRouteManager.GetRoutesAsync();
                descriptor = descriptors.FirstOrDefault(i => i.ServiceRouteDescriptor.Id == cacheId);
                if (descriptor != null)
                {
                    _concurrent.GetOrAdd(cacheId, descriptor);
                }
                else
                {
                    if (descriptor == null)
                    {
                        if (_logger.IsEnabled(LogLevel.Warning))
                            _logger.LogWarning($"根据缓存id：{cacheId}，找不到缓存信息。");
                        return null;
                    }
                }
            }

            if (descriptor.Address.Count() == 0)
            {
                if (_logger.IsEnabled(LogLevel.Warning))
                    _logger.LogWarning($"根据缓存id：{cacheId}，找不到可用的地址。");
                return null;
            }

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation($"根据缓存id：{cacheId}，找到以下可用地址：{string.Join(",", descriptor.Address.Select(i => i.ToString()))}。");
            var redisContext = _container.ResolveKeyed<RedisContext>(descriptor.ServiceRouteDescriptor.Id);
            ConsistentHash<RedisEndPoint> hash;
            redisContext.dicHash.TryGetValue("redis", out hash);
            return hash != null ? hash.GetItemNode(item) : default(RedisEndPoint);
        }


        private static string GetKey(ServiceRouteDescriptor descriptor)
        {
            return descriptor.Id;
        }

        private void ServiceRouteManager_Removed(object sender, ServiceRouteEventArgs e)
        {
            var key = GetKey(e.Route.ServiceRouteDescriptor);
            if (_container.IsRegisteredWithKey<RedisContext>(e.Route.ServiceRouteDescriptor.Id))
            {
                var redisContext = _container.ResolveKeyed<RedisContext>(e.Route.ServiceRouteDescriptor.Id);
                ServiceRoute value;
                _concurrent.TryRemove(key, out value);
                ConsistentHash<RedisEndPoint> hash;
                redisContext.dicHash.TryGetValue("redis", out hash);
                if (hash != null)
                    foreach (var node in e.Route.Address)
                    {

                        var hashNode = node as RedisEndPoint;
                        var addr = string.Format("{0}:{1}", hashNode.Host, hashNode.Port);
                        hash.Remove(addr);
                        hash.Add(hashNode, addr);
                    }
            }
        }

        private void ServiceRouteManager_Add(object sender, ServiceRouteEventArgs e)
        {
            var key = GetKey(e.Route.ServiceRouteDescriptor);
            if (_container.IsRegisteredWithKey<RedisContext>(e.Route.ServiceRouteDescriptor.Id))
            {
                var redisContext = _container.ResolveKeyed<RedisContext>(e.Route.ServiceRouteDescriptor.Id);
                _concurrent.GetOrAdd(key, e.Route);
                ConsistentHash<RedisEndPoint> hash;
                redisContext.dicHash.TryGetValue("redis", out hash);
                if (hash != null)
                    foreach (var node in e.Route.Address)
                    {
                        var hashNode = node as RedisEndPoint;
                        var addr = string.Format("{0}:{1}", hashNode.Host, hashNode.Port);
                        hash.Remove(addr);
                        hash.Add(hashNode, addr);
                    }
            }
        }
    }
}
