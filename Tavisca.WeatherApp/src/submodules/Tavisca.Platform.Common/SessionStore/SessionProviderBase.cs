using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Profiling;
using Tavisca.Platform.Common.Serialization;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using Tavisca.Platform.Common.SessionStore;
using Tavisca.Platform.Common.MemoryStreamPool;
using System.Diagnostics;
using Tavisca.Platform.Common.Context;
using Tavisca.Platform.Common.Logging;

namespace Tavisca.Common.Plugins.SessionStore
{
    public abstract class SessionProviderBase : ISessionDataProvider
    {
        private ISerializerFactory _serializerFactory;
        private const int _copyBuffer = 4096;

        private IMemoryStreamPool _memoryStreamPool;


        public SessionProviderBase(ISerializerFactory serializerFactory, IMemoryStreamPool memoryStreamPool)
        {
            _serializerFactory = serializerFactory;
            _memoryStreamPool = memoryStreamPool;
        }
        protected virtual async Task<byte[]> GetSerializedData(object data, ISerializer serializer, CancellationToken cancellationToken)
        {
            using (new ProfileContext("session data - serialize"))
            {
                using (var stream = await _memoryStreamPool.GetMemoryStream(cancellationToken))
                {
                    serializer.Serialize(stream, data);
                    return stream.ToArray();
                }
            }
        }

        protected virtual async Task<T> GetDeserializedData<T>(byte[] data, ISerializer serializer, CancellationToken cancellationToken)
        {
            using (new ProfileContext("session data - deserialize"))
            {
                using (var memoryStream = await _memoryStreamPool.GetMemoryStream(data, cancellationToken))
                {
                    return serializer.Deserialize<T>(memoryStream);
                }
            }
        }

        protected virtual async Task<byte[]> GetCompressedData(byte[] data, CancellationToken cancellationToken)
        {
            using (var dataStream = new MemoryStream(data))
            {
                var outputStream = new MemoryStream();

                using (var compressionStream = new DeflateStream(outputStream, CompressionMode.Compress))
                {
                    await dataStream.CopyToAsync(compressionStream, _copyBuffer, cancellationToken);
#if NET_STANDARD
                    compressionStream.Dispose();
#else
                    compressionStream.Close();
#endif
                    return outputStream.ToArray();
                }
            }
        }

        protected virtual async Task<byte[]> GetDecompressedData(byte[] data, CancellationToken cancellationToken)
        {
            var dataStream = new MemoryStream(data);
            using (var compressionStream = new DeflateStream(dataStream, CompressionMode.Decompress))
            {
                using (var outputStream = new MemoryStream())
                {
                    await compressionStream.CopyToAsync(outputStream, _copyBuffer, cancellationToken);
                    return outputStream.ToArray();
                }
            }
        }

        private async Task<T> DecompressAndDeserialize<T>(byte[] data, ISerializer serializer, CancellationToken cancellationToken)
        {
            var decompressedData = await GetDecompressedData(data, cancellationToken);

            return await GetDeserializedData<T>(decompressedData, serializer, cancellationToken);
        }

        public async Task AddAsync<T>(ItemKey itemKey, T value, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true)
        {
            if (value == null)
                return;

            if (itemKey == null)
                throw new ArgumentNullException(nameof(itemKey));

            var serializer = _serializerFactory.Create(typeof(T));
            var compressedData = await SerializeWithCompression<T>(value, serializer, cancellationToken, enableTraces);
            await Push(itemKey.Category, itemKey.Key, compressedData, cancellationToken, enableTraces);
        }

        public async Task AddMultipleAsync<T>(DataItem<T>[] items, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true)
        {
            if (items == null)
                throw new ArgumentNullException(nameof(items));

            var partitionCount = await GetPartitionCount(items.Length);
            await Task.WhenAll(
                Partitioner.Create(items).GetPartitions((int)(partitionCount))
                    .Select(partition => Task.Run(async () => await AddItemsFromPartitionAsync(partition, cancellationToken, enableTraces))
                ));

        }

        private async Task AddItemsFromPartitionAsync<T>(IEnumerator<DataItem<T>> partition, CancellationToken cancellationToken, bool enableTraces = true)
        {
            var items = await ConvertItemsAsync(partition, cancellationToken, enableTraces);
            await PushAll(items, cancellationToken, enableTraces);
        }

        private async Task<IDictionary<ItemKey, byte[]>> ConvertItemsAsync<T>(IEnumerator<DataItem<T>> partition, CancellationToken cancellationToken, bool enableTraces = true)
        {
            var dataLookUp = new Dictionary<ItemKey, byte[]>();
            var serializationTasks = new List<Task>();
            var serializer = _serializerFactory.Create(typeof(T));
            using (partition)
                while (partition.MoveNext())
                {
                    var element = partition.Current;

                    if (element.Value == null)
                        continue;

                    if (element.Key == null)
                        throw new ArgumentNullException(nameof(element.Key));

                    //processing each partition element in parallel task to improve performance
                    serializationTasks.Add(Task.Run(async () => 
                    {   var key = element.Key;
                        dataLookUp.Add(key, await SerializeWithCompression<T>(element.Value, serializer, cancellationToken, enableTraces));
                    }));
                }
            await Task.WhenAll(serializationTasks.Where(t => t != null));
            return dataLookUp;
        }

        public async Task<List<DataItem<T>>> GetAllAsync<T>(ItemKey[] itemKeys, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true)
        {
            if (itemKeys == null)
                throw new ArgumentNullException(nameof(itemKeys));
            if (itemKeys.Length == 0)
                return null;


            var sessionItems = (await FetchAll(itemKeys, cancellationToken, enableTraces)).ToList();
            var serializer = _serializerFactory.Create(typeof(T));
            //get partition count from provider
            var partitionCount = await GetPartitionCount(sessionItems.Count);
            var results = await Task.WhenAll(
            Partitioner.Create(sessionItems, true).GetPartitions(partitionCount)
                .Select(partition => Task.Run(async () =>
                {
                    var readItems = new LinkedList<DataItem<T>>();
                    using (partition)
                        while (partition.MoveNext())
                        {
                            var element = partition.Current;
                            var deserializedItem = element.Value == null
                            ? default(T)
                            : await DeserializeWithDecompression<T>(element.Value, serializer, cancellationToken, enableTraces);
                            var itemKey = element.Key;
                            var dataItem = new DataItem<T>(itemKey, deserializedItem);
                            readItems.AddLast(dataItem);
                        }
                    return readItems;
                })
            ));

            return results.SelectMany(pairs => pairs).ToList();
        }

        public async Task<T> GetAsync<T>(ItemKey itemKey, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true)
        {
            if (itemKey == null)
                throw new ArgumentNullException(nameof(itemKey));

            var data = await Fetch(itemKey, cancellationToken, enableTraces);
            if (data == null)
                return default(T);
            var serializer = _serializerFactory.Create(typeof(T));
            return await DeserializeWithDecompression<T>(data, serializer, cancellationToken, enableTraces);
        }

        public abstract Task<bool> RemoveAsync(ItemKey itemKey, CancellationToken cancellationToken, bool enableTraces = true);

        //we are using same stream for serialization and compression, this yields performance
        private async Task<byte[]> SerializeWithCompression<T>(T value, ISerializer serializer, CancellationToken cancellationToken, bool enableTraces = true)
        {
            using (new ProfileContext("session data - serialize with compression"))
            {
                Stopwatch stopwatch = null;
                if (enableTraces) { stopwatch = new Stopwatch(); stopwatch.Start(); }
                byte[] compressedData;

                using (var outputStream = await _memoryStreamPool.GetMemoryStream())
                {
                    using (var compressionStream = new DeflateStream(outputStream, CompressionMode.Compress, true))
                    {
                        serializer.Serialize(compressionStream, value);
                    }
                    compressedData = outputStream.ToArray();
                }
                if (enableTraces)
                {
                    stopwatch.Stop();
                    PutTimeTrace(stopwatch.ElapsedMilliseconds, "serialize_compression", compressedData.Length);
                }
                return compressedData;
            }
        }

        //we are using same stream for serialization and compression, this yields performance
        private async Task<T> DeserializeWithDecompression<T>(byte[] data, ISerializer serializer, CancellationToken cancellationToken, bool enableTraces = true)
        {
            T response;
            Stopwatch stopwatch = null;
            if (enableTraces) { stopwatch = new Stopwatch(); stopwatch.Start(); }
            using (var scope = new ProfileContext("session data - deserialize with decompression"))
            {
                using (var outputStream = await _memoryStreamPool.GetMemoryStream(data))
                {
                    using (var compressionStream = new DeflateStream(outputStream, CompressionMode.Decompress))
                    {
                        response = serializer.Deserialize<T>(compressionStream);
                    }
                }
            }
            if (enableTraces)
            {
                stopwatch.Stop();
                PutTimeTrace(stopwatch.ElapsedMilliseconds, "deserialize_decompression");
            }
            return response;
        }

        private void PutTimeTrace(long elapsedMilliseconds, string category, int length = 0)
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
            trace.SetValue("data_length", length);
            Logger.WriteLogAsync(trace);
        }

        protected abstract Task Push(string category, string key, byte[] value, CancellationToken cancellationToken, bool enableTraces = true);
        protected abstract Task PushAll(IDictionary<ItemKey, byte[]> items, CancellationToken cancellationToken, bool enableTraces = true);
        protected abstract Task<byte[]> Fetch(ItemKey itemKey, CancellationToken cancellationToken, bool enableTraces = true);
        protected abstract Task<IDictionary<ItemKey, byte[]>> FetchAll(ItemKey[] keys, CancellationToken cancellationToken, bool enableTraces = true);
        protected abstract Task<int> GetPartitionCount(int totalCount);
    }
}
