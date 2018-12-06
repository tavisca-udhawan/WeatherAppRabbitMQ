using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common;
using Tavisca.Platform.Common.Cache;
using Tavisca.Platform.Common.Profiling;

namespace Tavisca.Common.Plugins.Redis
{
    public class RedisCacheProvider<T> : ICache<T>
    {
        readonly IRedisClientFactory _clientFactory;
        ICacheSettingsProvider _cacheSettingsProvider;

        public RedisCacheProvider(IRedisClientFactory redisFactory, ICacheSettingsProvider cacheSettingsProvider)
        {
            _clientFactory = redisFactory;
            _cacheSettingsProvider = cacheSettingsProvider;
        }

        private IRedisClient GetClient(RedisSettings settings)
        {
            return _clientFactory.Create(settings);
        }

        public async Task DeleteAsync(string key, CancellationToken token = default(CancellationToken))
        {
            using (new ProfileContext(string.Format("Delete key : {0}", key)))
            {
                var settings = await _cacheSettingsProvider.GetCacheSettingsAsync() as RedisSettings;
                if (settings.IsDisabled == true) return;

                var cancellationSource = new CancellationTokenSource();
                var task = Retry.ExecuteWithFaultSuppressionAsync(async () =>
                {
                    if (cancellationSource.IsCancellationRequested == true)
                        return;
                    using (var client = GetClient(settings))
                    {
                        client.Remove(key);
                    }
                });
                var cancellationTask = Task.Delay(TimeSpan.FromMilliseconds(settings.TimeoutInMs)).ContinueWith(x => cancellationSource.Cancel());
                await Task.WhenAny(task, cancellationTask);
            }
        }

        public async Task<T> GetAsync(string key, CancellationToken token = default(CancellationToken), Func<byte[], T> deserialize = null)
        {
            T value = default(T);
            using (new ProfileContext(string.Format("Get key : {0}", key)))
            {
                var settings = await _cacheSettingsProvider.GetCacheSettingsAsync() as RedisSettings;
                if (settings.IsDisabled == true) return value;

                var cancellationSource = new CancellationTokenSource();
                var task = Retry.ExecuteWithFaultSuppressionAsync(async () =>
                {
                    if (cancellationSource.IsCancellationRequested)
                        return;
                    using (var client = GetClient(settings))
                    {
                        if (deserialize != null)
                        {
                            value = deserialize(client.Get<byte[]>(key));
                        }
                        else
                            value = client.Get<T>(key);
                    }
                });
                var cancellationTask = Task.Delay(TimeSpan.FromMilliseconds(settings.TimeoutInMs)).ContinueWith(x => cancellationSource.Cancel());
                await Task.WhenAny(task, cancellationTask);
                return value;
            }
        }

        public async Task<List<CacheHit<T>>> MultiGetAsync(List<string> keys, CancellationToken token = default(CancellationToken), Func<byte[], T> deserialize = null)
        {
            using (new ProfileContext("Multi get"))
            {
                var response = keys.Select(k => new CacheHit<T>(k, default(T))).ToList();
                var settings = await _cacheSettingsProvider.GetCacheSettingsAsync() as RedisSettings;
                if (settings.IsDisabled == true) return response;

                var cancellationSource = new CancellationTokenSource();
                var task = Retry.ExecuteWithFaultSuppressionAsync(async () => {
                    if (cancellationSource.IsCancellationRequested)
                        return;

                    using (var client = GetClient(settings))
                    {
                        var values = new List<CacheHit<T>>();
                        if (deserialize != null)
                        {
                            var results = client.GetAll<byte[]>(keys);
                            foreach (var e in results)
                            {
                                if (e.Value != null)
                                {
                                    values.Add(new CacheHit<T>(e.Key, deserialize(e.Value)));
                                }
                                else
                                {
                                    values.Add(new CacheHit<T>(e.Key, default(T)));
                                }
                            }
                        }
                        else
                        {
                            var result = client.GetAll<T>(keys);
                            foreach (var e in result)
                            {
                                if (e.Value != null)
                                {
                                    values.Add(new CacheHit<T>(e.Key, e.Value));
                                }
                                else
                                {
                                    values.Add(new CacheHit<T>(e.Key, default(T)));
                                }
                            }
                        }
                        response = values;
                    }
                });
                var cancellationTask = Task.Delay(TimeSpan.FromMilliseconds(settings.TimeoutInMs)).ContinueWith(x => cancellationSource.Cancel());
                await Task.WhenAny(task, cancellationTask);
                return response;
            }
        }

        public async Task SetAsync(string key, T value, CancellationToken token = default(CancellationToken), Func<T, byte[]> serialize = null, TimeSpan? expiresIn = null)
        {
            using (new ProfileContext(string.Format("Set key : {0}", key)))
            {
                var settings = await _cacheSettingsProvider.GetCacheSettingsAsync() as RedisSettings;
                if (settings.IsDisabled == true) return;

                var cancellationSource = new CancellationTokenSource();
                var task = Retry.ExecuteWithFaultSuppressionAsync(async () =>
                {
                    if (cancellationSource.IsCancellationRequested == true)
                        return;
                    using (var client = GetClient(settings))
                    {
                        if (serialize != null)
                        {
                            var byteArray = serialize(value);
                            if (expiresIn.HasValue)
                                client.Set(key, byteArray, expiresIn.Value);
                            else
                                client.Set(key, byteArray);
                        }
                        else
                        {
                            if(expiresIn.HasValue)
                                client.Set(key, value, expiresIn.Value);
                            else
                                client.Set(key, value);
                        }
                    }
                });
                var cancellationTask = Task.Delay(TimeSpan.FromMilliseconds(settings.TimeoutInMs)).ContinueWith(x => cancellationSource.Cancel());
                await Task.WhenAny(task, cancellationTask);
            }
        }
    }
}
