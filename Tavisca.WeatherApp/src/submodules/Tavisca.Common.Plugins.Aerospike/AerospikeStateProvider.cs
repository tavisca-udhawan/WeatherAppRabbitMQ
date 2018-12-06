using Aerospike.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.SessionStore;
using Tavisca.Platform.Common;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.Context;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.MemoryStreamPool;
using Tavisca.Platform.Common.Profiling;
using Tavisca.Platform.Common.Serialization;
using Tavisca.Platform.Common.SessionStore;

namespace Tavisca.Common.Plugins.Aerospike
{
    public class AerospikeStateProvider : ISessionDataProvider
    {
        private ISerializerFactory _serializerFactory;
        private IMemoryStreamPool _memoryStreamPool;
        private IConfigurationProvider _configurationProvider;
        private readonly IAerospikeClientFactory _clientFactory;
        private static WritePolicy _writePolicy;
        private static QueryPolicy _queryPolicy;
        private const string ConfigurationSection = "framework_settings";
        private const string ConfigurationKey = "data_store";

        public AerospikeStateProvider(IAerospikeClientFactory clientFactory, IConfigurationProvider configurationProvider, ISerializerFactory serializerFactory, IMemoryStreamPool memoryStreamPool)
        {
            _serializerFactory = serializerFactory;
            _memoryStreamPool = memoryStreamPool;
            _configurationProvider = configurationProvider;
            _clientFactory = clientFactory;
        }

        public async Task AddAsync<T>(ItemKey itemKey, T value, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true)
        {
            Stopwatch rootStopwatch = null;
            if (enableTraces) { rootStopwatch = new Stopwatch(); rootStopwatch.Start(); }
            using (new ProfileContext("Pushing state to Aerospike"))
            {
                var sessionConfiguration = await GetSessionConfiguration();
                var settings = await GetSettings();
                var bins = new Bin[] { new Bin(Keystore.AerospikeKeys.BinNames.ObjectState, await Serialize<T>(value)) };
                var client = GetClient(settings);
                using (new ProfileContext("aerospike client put call"))
                    await client.Put(GetWritePolicy(sessionConfiguration.ExpiresIn), cancellationToken, GetKey(settings.Namespace, itemKey.Category, itemKey.Key), bins);
            }
            if (enableTraces)
            {
                rootStopwatch.Stop();
                PutTimeTrace(rootStopwatch.ElapsedMilliseconds, "aerospike_state_put");
            }
        }


        public async Task AddMultipleAsync<T>(DataItem<T>[] items, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true)
        {
            Stopwatch rootStopwatch = null;
            if (enableTraces) { rootStopwatch = new Stopwatch(); rootStopwatch.Start(); }
            using (new ProfileContext("PushingAll state to Aerospike"))
            {
                var sessionConfiguration = await GetSessionConfiguration();
                var settings = await GetSettings();
                var client = GetClient(settings);
                foreach (var item in items)
                {
                    var itemKey = item.Key;
                    var value = item.Value;
                    var bins = new Bin[] { new Bin(Keystore.AerospikeKeys.BinNames.ObjectState, await Serialize<T>(value)) };
                    using (new ProfileContext("aerospike client putall call"))
                        await client.Put(GetWritePolicy(sessionConfiguration.ExpiresIn), cancellationToken, GetKey(settings.Namespace, itemKey.Category, itemKey.Key), bins);
                }
                if (enableTraces)
                {
                    rootStopwatch.Stop();
                    PutTimeTrace(rootStopwatch.ElapsedMilliseconds, "aerospike_state_putall", items.Length);
                }
            }
        }

        public async Task<List<DataItem<T>>> GetAllAsync<T>(ItemKey[] itemKeys, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true)
        {
            Stopwatch rootStopwatch = null;
            if (enableTraces) { rootStopwatch = new Stopwatch(); rootStopwatch.Start(); }
            using (new ProfileContext("FetchAll state from Aerospike"))
            {
                var settings = await GetSettings();
                var providerKeys = ArrayExtension.ConvertAll(itemKeys, x => GetKey(settings.Namespace, x.Category, x.Key));
                var client = GetClient(settings);
                Record[] records;

                using (new ProfileContext("aerospike client getall call"))
                    records = await client.Get(null, cancellationToken, providerKeys);

                records = records ?? Array.Empty<Record>();

                var valueMap = new List<DataItem<T>>();
                for (var i = 0; i < itemKeys.Length; i++)
                {
                    var record = records[i];
                    if (record == null)
                        continue;
                    var data = (byte[])record.GetValue(Keystore.AerospikeKeys.BinNames.ObjectState);
                    valueMap.Add(new DataItem<T>(itemKeys[i], await Deserialize<T>(data)));
                }
                if (enableTraces)
                {
                    rootStopwatch.Stop();
                    PutTimeTrace(rootStopwatch.ElapsedMilliseconds, "aerospike_state_fetchall", valueMap.Count);
                }
                return valueMap;
            }

        }

        public async Task<T> GetAsync<T>(ItemKey itemKey, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true)
        {
            Stopwatch rootStopwatch = null;
            if (enableTraces) { rootStopwatch = new Stopwatch(); rootStopwatch.Start(); }
            using (new ProfileContext("Fetch state from Aerospike"))
            {
                var settings = await GetSettings();
                var client = GetClient(settings);
                Record record;

                using (new ProfileContext("aerospike client get call"))
                    record = await client.Get(GetQueryPolicy(), cancellationToken, GetKey(settings.Namespace, itemKey.Category, itemKey.Key));
                if (record == null)
                    return default(T);

                if (enableTraces)
                {
                    rootStopwatch.Stop();
                    PutTimeTrace(rootStopwatch.ElapsedMilliseconds, "aerospike_state_fetch");
                }
                return await Deserialize<T>((byte[])record.GetValue(Keystore.AerospikeKeys.BinNames.ObjectState));
            }
        }

        public async Task<bool> RemoveAsync(ItemKey itemKey, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true)
        {
            var settings = await GetSettings();
            var client = GetClient(settings);
            using (new ProfileContext("aerospike client remove call"))
                return await client.Delete(GetWritePolicy(null), cancellationToken, GetKey(settings.Namespace, itemKey.Category, itemKey.Key));
        }

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


        private WritePolicy GetWritePolicy(TimeSpan? expiry)
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

        private Key GetKey(string ns, string category, string key)
        {
            return new Key(ns, category, key);
        }

        private AsyncClient GetClient(AerospikeSettings settings)
        {
            return _clientFactory.GetClient(settings.Host, settings.Port, settings.SecondaryHosts);
        }

        private async Task<byte[]> Serialize<T>(T value)
        {
            byte[] serializedValue;

            if (value is byte[])
                return serializedValue = value as byte[];

            var serializer = _serializerFactory.Create(typeof(T));

            using (new ProfileContext("State serialization"))
            using (var outputStream = await _memoryStreamPool.GetMemoryStream())
            {
                serializer.Serialize(outputStream, value);
                serializedValue = outputStream.ToArray();
            }

            return serializedValue;
        }

        private async Task<T> Deserialize<T>(object value)
        {
            if (typeof(T).Equals(typeof(byte[])))
                return (T)value;

            var serializer = _serializerFactory.Create(typeof(T));
            using (new ProfileContext("State deserialization"))
                return serializer.Deserialize<T>(await _memoryStreamPool.GetMemoryStream((byte[])value, CancellationToken.None));
        }

        private Task<AerospikeSettings> GetSettings()
        {
            using (new ProfileContext("fetching aerospike(stateProvider) settings"))
                return _configurationProvider.GetGlobalConfigurationAsync<AerospikeSettings>(
                Keystore.AerospikeKeys.SettingsSection, Keystore.AerospikeKeys.StateSettings);
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
    }
}
