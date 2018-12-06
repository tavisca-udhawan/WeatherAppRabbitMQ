
using System;
using System.Collections.Generic;
using System.Threading;
using ServiceStack.Redis;

namespace Tavisca.Common.Plugins.Redis
{
    public class RedisClientFactory : IRedisClientFactory
    {
        private static readonly Dictionary<string, RedisManagerPool> Cache = new Dictionary<string, RedisManagerPool>(StringComparer.OrdinalIgnoreCase);
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

        public IRedisClient Create(RedisSettings settings)
        {
            RedisManagerPool client = null;
            Lock.EnterUpgradeableReadLock();
            try
            {
                if (Cache.TryGetValue(settings.Signature, out client) == false)
                {
                    Lock.EnterWriteLock();
                    try
                    {
                        if (Cache.TryGetValue(settings.Signature, out client) == false)
                        {
                            client = new RedisManagerPool(settings.Hosts.ToArray());

                            Cache[settings.Signature] = client;
                        }
                    }
                    finally
                    {
                        Lock.ExitWriteLock();
                    }
                }
            }
            finally
            {
                Lock.ExitUpgradeableReadLock();
            }

            return client.GetClient();
        }

    }
}
