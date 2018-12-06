using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Tavisca.Platform.Common.Internal;

namespace Tavisca.Platform.Common
{
    public static class AsyncTasks
    {
        public static void UseRoundRobinPool()
        {
            Func<int, ITaskPool> func = x => new RoundRobinPool(x);
            Interlocked.Exchange(ref _createPool, func);
        }

        public static void UseDefaultPool()
        {
            Func<int, ITaskPool> func = x => new TaskPool(x);
            Interlocked.Exchange(ref _createPool, func);
        }

        public static readonly int DefaultPoolSize = Environment.ProcessorCount;
        private static readonly string DefaultPoolName = "{AA316252-1D20-4607-A55A-293ED8D38CE9}";
        private static ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private static readonly Dictionary<string, ITaskPool> _pools = new Dictionary<string, ITaskPool>(StringComparer.OrdinalIgnoreCase);
        private static Func<int, ITaskPool> _createPool = x => new TaskPool(x);


        public static void AddPool(string name, int size)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_pools.ContainsKey(name) == false)
                    _pools[name] = _createPool(size);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public static void AddPool(string name, ITaskPool pool)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_pools.ContainsKey(name) == false)
                    _pools[name] = pool;
                else throw new InvalidOperationException($"Pool with name={name} already exists.");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public static void Run(Action action, string poolName = null)
        {
            string name = poolName;
            if (string.IsNullOrWhiteSpace(name) == true)
                name = DefaultPoolName;
            var pool = GetPool(name);
            pool.Enqueue(action);
        }

        private static ITaskPool GetPool(string name)
        {
            ITaskPool pool = null;
            _lock.EnterUpgradeableReadLock();
            try
            {
                if(_pools.TryGetValue(name, out pool) == false )
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        if (_pools.TryGetValue(name, out pool) == false)
                        {
                            pool = _createPool(DefaultPoolSize);
                            _pools[name] = pool;
                        }
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
            return pool;
        }


        public static void Stop()
        {
            var copy = _pools;
            _pools.Clear();
            if( copy != null )
            {
                foreach (var pool in copy.Values)
                    pool.StopAdding();
            }
        }
    }
}
