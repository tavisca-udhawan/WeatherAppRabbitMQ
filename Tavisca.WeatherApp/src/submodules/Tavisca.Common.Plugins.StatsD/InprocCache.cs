using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.Metrics;

namespace Tavisca.Common.Plugins.StatsD
{
    internal class InprocCache<T> : IDisposable
    {
        public InprocCache(TimeSpan lifetime)
        {
            CacheLifetime = lifetime;
        }
    
        private static Dictionary<string, T> _cache = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        public T GetExistingOrCreate(string key, Func<T> createNew)
        {
            try
            {
                return GetExistingOrCreateInternal(key, createNew);
            }
            finally
            {
                FlushCacheIfExpired();
            }
        }

        public TimeSpan CacheLifetime { get; set; } = new TimeSpan(0, 10, 0);
        private static DateTime _lastFlushed = DateTime.UtcNow;

        private void FlushCacheIfExpired()
        {
            List<T> existing = null;
            if (_lastFlushed.Add(CacheLifetime) > DateTime.UtcNow)
                return;

            _lock.EnterWriteLock();
            try
            {
                existing = _cache.Values.ToList();
                _cache.Clear();
                _lastFlushed = DateTime.UtcNow;
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            // Dispose all objects
            existing
                .FindAll(x => x is IDisposable)
                .ForEach(x =>
                {
                    try { (x as IDisposable).Dispose(); } catch { }
                });
        }

        private T GetExistingOrCreateInternal(string key, Func<T> createNew)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                T obj = default(T);
                if (_cache.TryGetValue(key, out obj) == false)
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        if (_cache.TryGetValue(key, out obj) == false)
                        {
                            obj = createNew();
                            _cache[key] = obj;
                        }
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
                return obj;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    var copy = _cache;
                    _cache = null;
                    if (copy != null)
                    {
                        foreach (var value in copy.Values.ToArray())
                        {
                            var disposable = value as IDisposable;
                            if (disposable != null)
                            {
                                try
                                {
                                    disposable.Dispose();
                                }
                                catch { }
                            }
                        }
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            
        }
        

    }
}
