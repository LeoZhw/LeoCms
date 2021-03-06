﻿using Leo.Microservice.Abstractions.Cache;
using Leo.Microservice.Abstractions.Cache.HashAlgorithms;
using Leo.Microservice.Utils;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Leo.Microservice.Redis
{
    public class RedisCacheClient<T> : ICacheClient<T>
            where T : class
    {
        private static readonly ConcurrentDictionary<string, Lazy<ObjectPool<T>>> _pool =
            new ConcurrentDictionary<string, Lazy<ObjectPool<T>>>();

        public RedisCacheClient()
        {

        }

        public async Task<bool> ConnectionAsync(EndPoint endpoint, int connectTimeout)
        {
            ConnectionMultiplexer conn = null;
            try
            {
                var info = endpoint as RedisEndPoint;
                var point = string.Format("{0}:{1}", info.Host, info.Port);
                conn = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions()
                {
                    EndPoints = { { point } },
                    ServiceName = point,
                    Password = info.Password,
                    ConnectTimeout = connectTimeout
                });
                return conn.IsConnected;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        public T GetClient(EndPoint endpoint, int connectTimeout)
        {
            try
            {
                var info = endpoint as RedisEndPoint;
                Check.NotNull(info, "endpoint");
                var key = string.Format("{0}{1}{2}{3}", info.Host, info.Port, info.Password, info.DbIndex);
                if (!_pool.ContainsKey(key))
                {
                    var objectPool = new Lazy<ObjectPool<T>>(() => new ObjectPool<T>(() =>
                    {
                        var point = string.Format("{0}:{1}", info.Host, info.Port);
                        var redisClient = ConnectionMultiplexer.Connect(new ConfigurationOptions()
                        {
                            EndPoints = { { point } },
                            ServiceName = point,
                            Password = info.Password,
                            ConnectTimeout = connectTimeout,
                            AbortOnConnectFail = false
                        });
                        return redisClient.GetDatabase(info.DbIndex) as T;
                    }, info.MinSize, info.MaxSize));
                    _pool.GetOrAdd(key, objectPool);
                    return objectPool.Value.GetObject();
                }
                else
                {
                    return _pool[key].Value.GetObject();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
