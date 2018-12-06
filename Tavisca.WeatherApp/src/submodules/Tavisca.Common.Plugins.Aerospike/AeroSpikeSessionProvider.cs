using Aerospike.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Serialization;
using System.Collections.Generic;
using System.Linq;
using Tavisca.Platform.Common.SessionStore;
using Tavisca.Platform.Common.MemoryStreamPool;
using Tavisca.Common.Plugins.SessionStore;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.Profiling;
using System.Collections.Concurrent;
using Tavisca.Platform.Common;
using System.Diagnostics;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Context;

namespace Tavisca.Common.Plugins.Aerospike
{
    public class AeroSpikeSessionProvider : SessionProviderBase
    {
        private const string ConfigurationSection = "framework_settings";
        private const string ConfigurationKey = "data_store";
        private const string _dataBin = "bin1";

        private readonly IAerospikeClientFactory _clientFactory;
        private readonly IConfigurationProvider _configurationProvider;

        public AeroSpikeSessionProvider(IAerospikeClientFactory clientFactory, IConfigurationProvider configurationProvider, ISerializerFactory serializerFactory, IMemoryStreamPool memoryStreamPool)
            : base(serializerFactory, memoryStreamPool)
        {
            _clientFactory = clientFactory;
            _configurationProvider = configurationProvider;
        }

        protected override async Task Push(string category, string key, byte[] value, CancellationToken cancellationToken, bool enableTraces = true)
        {
            Stopwatch rootStopwatch = null;
            if (enableTraces) { rootStopwatch = new Stopwatch(); rootStopwatch.Start(); }
            using (new ProfileContext("Pushing to Aerospike"))
            {
                var sessionConfiguration = await GetSessionConfiguration();
                var settings = await GetSettings();
                var bins = new Bin[] { new Bin(_dataBin, value) };
                var client = GetClient(settings);
                await client.Put(GetWritePolicy(sessionConfiguration.ExpiresIn), cancellationToken, GetKey(settings.Namespace, category, key), bins);
            }
            if (enableTraces)
            {
                rootStopwatch.Stop();
                PutTimeTrace(rootStopwatch.ElapsedMilliseconds, "aerospike_put");
            }
        }

        private void PutTimeTrace(long elapsedMilliseconds, string category, int itemCount = 0)
        {
            var context = CallContext.Current;
            var trace = new TraceLog
            {
                ApplicationName = context?.ApplicationName,
                StackId = context?.StackId,
                TenantId = context?.TenantId,
                CorrelationId = context?.CorrelationId,
                ApplicationTransactionId = context?.TransactionId,
                Category = category
            };
            trace.SetValue("time_taken_ms", elapsedMilliseconds);
            trace.SetValue("item_count", itemCount);
            Logger.WriteLogAsync(trace);
        }

        protected override async Task PushAll(IDictionary<ItemKey, byte[]> items, CancellationToken cancellationToken, bool enableTraces = true)
        {
            Stopwatch rootStopwatch = null;
            if (enableTraces) { rootStopwatch = new Stopwatch(); rootStopwatch.Start(); }
            using (new ProfileContext("PushingAll to Aerospike"))
            {
                var sessionConfiguration = await GetSessionConfiguration();
                var settings = await GetSettings();
                var client = GetClient(settings);
                foreach (var item in items)
                {
                    var itemKey = item.Key;
                    var value = item.Value;
                    var bins = new Bin[] { new Bin(_dataBin, value) };

                    await client.Put(GetWritePolicy(sessionConfiguration.ExpiresIn), cancellationToken, GetKey(settings.Namespace, itemKey.Category, itemKey.Key), bins);
                }
                if (enableTraces)
                {
                    rootStopwatch.Stop();
                    PutTimeTrace(rootStopwatch.ElapsedMilliseconds, "aerospike_putall", items.Count);
                }
            }
        }
        protected override async Task<byte[]> Fetch(ItemKey itemKey, CancellationToken cancellationToken, bool enableTraces = true)
        {
            Stopwatch rootStopwatch = null;
            if (enableTraces) { rootStopwatch = new Stopwatch(); rootStopwatch.Start(); }
            using (new ProfileContext("Fetch from Aerospike"))
            {
                var settings = await GetSettings();
                var client = GetClient(settings);
                var record = await client.Get(GetQueryPolicy(), cancellationToken, GetKey(settings.Namespace, itemKey.Category, itemKey.Key));
                if (record == null)
                    return null;
                if (enableTraces)
                {
                    rootStopwatch.Stop();
                    PutTimeTrace(rootStopwatch.ElapsedMilliseconds, "aerospike_fetch");
                }
                return (byte[])record.GetValue(_dataBin);
            }
        }

        protected override async Task<IDictionary<ItemKey, byte[]>> FetchAll(ItemKey[] keys, CancellationToken cancellationToken, bool enableTraces = true)
        {
            Stopwatch rootStopwatch = null;
            if (enableTraces) { rootStopwatch = new Stopwatch(); rootStopwatch.Start(); }
            using (new ProfileContext("FetchAll from Aerospike"))
            {
                var settings = await GetSettings();
                var providerKeys = ArrayExtension.ConvertAll(keys, x => GetKey(settings.Namespace, x.Category, x.Key));
                var client = GetClient(settings);

                var records = await client.Get(null, cancellationToken, providerKeys);
                
                records = records ?? Array.Empty<Record>();

                var valueMap = new Dictionary<ItemKey, byte[]>();
                for (var i = 0; i < keys.Length; i++)
                {
                    var record = records[i];
                    if (record == null)
                        continue;
                    var data = (byte[])record.GetValue(_dataBin);
                    valueMap.Add(keys[i], data);
                }
                if (enableTraces)
                {
                    rootStopwatch.Stop();
                    PutTimeTrace(rootStopwatch.ElapsedMilliseconds, "aerospike_fetchall", valueMap.Count);
                }
                return valueMap;
            }
        }

        public override async Task<bool> RemoveAsync(ItemKey itemKey, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true)
        {
            if (string.IsNullOrWhiteSpace(itemKey.Category))
                throw new ArgumentNullException(nameof(itemKey.Category));

            if (string.IsNullOrWhiteSpace(itemKey.Key))
                throw new ArgumentNullException(nameof(itemKey.Key));


            var settings = await GetSettings();
            var client = GetClient(settings);
            return await client.Delete(GetWritePolicy(null), cancellationToken, GetKey(settings.Namespace, itemKey.Category, itemKey.Key));
        }

        private static WritePolicy _writePolicy;
        protected virtual WritePolicy GetWritePolicy(TimeSpan? expiry)
        {
            var expiration = expiry.HasValue ? (int)expiry.Value.TotalSeconds : -1;
            return _writePolicy ?? (_writePolicy = new WritePolicy()
            {
                expiration = expiration,
                maxRetries = 2,
                priority = Priority.DEFAULT,
                sleepBetweenRetries = 50,
                sendKey = true
            });
        }

        private static QueryPolicy _queryPolicy;
      

        protected virtual QueryPolicy GetQueryPolicy()
        {
            return _queryPolicy ?? (_queryPolicy = new QueryPolicy()
            {
                maxRetries = 2,
                priority = Priority.DEFAULT,
                sleepBetweenRetries = 50,
                recordQueueSize = 1
            });
        }

      
        protected virtual Key GetKey(string ns, string category, string key)
        {
            return new Key(ns, category, key);
        }

        private AsyncClient GetClient(AerospikeSettings settings)
        {
            return _clientFactory.GetClient(settings.Host, settings.Port, settings.SecondaryHosts);
        }
        private Task<AerospikeSettings> GetSettings()
        {
            using (new ProfileContext("fetching aerospike(session data provider) settings"))
                return _configurationProvider.GetGlobalConfigurationAsync<AerospikeSettings>(
                Keystore.AerospikeKeys.SettingsSection, Keystore.AerospikeKeys.SessionProviderSettings);
        }
        protected override async Task<int> GetPartitionCount(int totalCount)
        {
            var sessionConfig = await GetSessionConfiguration();
            return Utility.GetPartitionCount(totalCount, sessionConfig.MaxItemsPerAsyncQueue);
        }
        private async Task<SessionConfiguration> GetSessionConfiguration()
        {
            var externalConfiguration =
                await _configurationProvider.GetGlobalConfigurationAsync<ExternalConfiguration>(ConfigurationSection, ConfigurationKey);
            var configuration = new SessionConfiguration()
            {
                MaxItemsPerAsyncQueue = externalConfiguration.MaxItemsPerAsyncQueue > 0
                                ? externalConfiguration.MaxItemsPerAsyncQueue : 5
            };

            var expiry = externalConfiguration.ExpiryInSeconds;

            configuration.ExpiresIn = (expiry == 0) ? TimeSpan.FromSeconds(3600) : TimeSpan.FromSeconds(expiry);
            return configuration;
        }
    }
}
