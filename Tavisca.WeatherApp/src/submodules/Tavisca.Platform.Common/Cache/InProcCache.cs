using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Context;
using Tavisca.Platform.Common.Logging;

namespace Tavisca.Platform.Common.Cache
{
    public class InProcCache<T> : ICache<T> where T : class
    {
        
        private ConcurrentDictionary<string, WeakReference<Tuple<byte[], DateTime>>> _cache = new ConcurrentDictionary<string, WeakReference<Tuple<byte[], DateTime>>>(StringComparer.OrdinalIgnoreCase);

        private static readonly T NullValue = default(T);

        public async Task<T> GetAsync(string key, CancellationToken cancellationToken = default(CancellationToken), Func<byte[], T> deserialize = null)
        {
            var result = Get(key, deserialize);
            if(result==null)
            {
                await WriteCacheMissTrace();
                return NullValue;
            }
            return result;
        }

        private async Task WriteCacheMissTrace()
        {
            var callContext = CallContext.Current;
            var log = new TraceLog
            {
                ApplicationName = callContext?.ApplicationName,
                TenantId = callContext?.TenantId,
                CorrelationId = callContext?.CorrelationId,
                StackId = callContext?.StackId,
                ApplicationTransactionId = callContext?.TransactionId,
                Category = "cache_miss",
                Message = "Cache returned null"
            };
            log.SetValue("cache_type", "inproc");
            await Logger.WriteLogAsync(log);
        }

        private T Get(string key, Func<byte[], T> deserialize)
        {
            WeakReference<Tuple<byte[], DateTime>> reference;
            if (_cache.TryGetValue(key, out reference) == false)
                return null;
            Tuple<byte[], DateTime> value;
            if (reference.TryGetTarget(out value) == false || value.Item1 == null)
                return null;
            if (value.Item2 > DateTime.UtcNow)
                return Deserialize(value.Item1, deserialize);
            else
            {
                _cache.TryRemove(key, out reference);
                return null;
            }
        }

        public Task SetAsync(string key, T value, CancellationToken cancellationToken = default(CancellationToken), Func<T, byte[]> serialize = null, TimeSpan? expiresIn = null)
        {
            var byteArray = Serialize(value,serialize);
            var reference = new WeakReference<Tuple<byte[], DateTime>>(new Tuple<byte[], DateTime>(byteArray, (expiresIn.HasValue ?  DateTime.UtcNow.Add(expiresIn.Value) : DateTime.MaxValue)));
            _cache.AddOrUpdate(key, reference, (k, v) => reference);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string key, CancellationToken cancellationToken = default(CancellationToken))
        {
            WeakReference <Tuple<byte[], DateTime>> value;
            _cache.TryRemove(key, out value);
            return Task.CompletedTask;
        }

        public Task<List<CacheHit<T>>> MultiGetAsync(List<string> keys, CancellationToken cancellationToken = default(CancellationToken), Func<byte[], T> deserialize = null)
        {
            var result = new List<CacheHit<T>>();
            keys.ForEach(async x =>
            {
                var cachedValue = Get(x, deserialize);
                if(cachedValue==null)
                {
                    await WriteCacheMissTrace();
                    cachedValue = default(T);
                }
                result.Add(new CacheHit<T>(x, cachedValue));
            });
            return Task.FromResult(result);
        }

        private byte[] Serialize(T obj, Func<T, byte[]> serialize)
        {
            if (serialize != null)
                return serialize(obj);

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private T Deserialize(byte[] arrBytes, Func<byte[], T> deserialize)
        {
            if (deserialize != null)
            {
                return deserialize(arrBytes);
            }

            using (MemoryStream memStream = new MemoryStream())
            {
                BinaryFormatter binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                var obj = (T)binForm.Deserialize(memStream);

                return obj;
            }
        }
    }
}