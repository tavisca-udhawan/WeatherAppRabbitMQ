using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aerospike.Client;

namespace Tavisca.Common.Plugins.Aerospike
{
    public class AerospikeClientFactory : IAerospikeClientFactory
    {
        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private static readonly IDictionary<string, AsyncClient> _cachedClientDictionary = new ConcurrentDictionary<string, AsyncClient>();

        public AsyncClient GetClient(string host, int port)
        {
            var client = GetAsyncClient(host, port, new List<string>());
            return client;
        }

        public  AsyncClient GetClient(string host, int port, List<string> secondaryHosts)
        {
            var client = GetAsyncClient(host, port, secondaryHosts);
            return client;
        }
        private AsyncClient GetAsyncClient(string host, int port, List<string> secondaryHosts)
        {
            AsyncClient client;
            var clientKey = GetClientKey(host, port, secondaryHosts);
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_cachedClientDictionary.TryGetValue(clientKey, out client) == false)
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        if (_cachedClientDictionary.TryGetValue(clientKey, out client) == false)
                        {
                            client = GetNewClient(host, port, secondaryHosts);
                            _cachedClientDictionary[clientKey] = client;
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
            if (client.Connected == false)
                client = RegenerateClient(host, port, secondaryHosts);

            return client;
        }   

        private AsyncClient RegenerateClient(string host, int port, List<string> secondaryHosts)
        {
            AsyncClient existing = null;
            var clientKey = GetClientKey(host, port, secondaryHosts); 
            _lock.EnterWriteLock();
            try
            {
                AsyncClient client = null;
                if (_cachedClientDictionary.TryGetValue(clientKey, out existing) == false)
                {
                    client = GetNewClient(host, port, secondaryHosts);
                    _cachedClientDictionary[clientKey] = client;
                    return client;
                }
                if (existing.Connected == false)
                {
                    existing.Dispose();
                    _cachedClientDictionary.Remove(clientKey);

                    client = GetNewClient(host, port, secondaryHosts);
                    _cachedClientDictionary[clientKey] = client;
                    return client;
                }
            }
            finally
            {
                _lock.ExitWriteLock();

            }
            return existing;

        }
        private string GetClientKey(string host, int port, List<string> secondaryHosts)
        {
            if(secondaryHosts.Count == 0)            
                return string.Concat(host.ToLower(), port);           
            return string.Concat(host.ToLower(), secondaryHosts.Aggregate((hostOne, hostTwo) => hostOne + hostTwo).ToLower(),port);
        }

        private static AsyncClient GetNewClient(string host, int port,List<string> secondaryHosts)
        {
            Host[] aerospikeHosts = GetHosts(host, port, secondaryHosts);           
            return new AsyncClient(null, aerospikeHosts);          
        }

        private static Host[] GetHosts(string primaryHost, int port, List<string> secondaryHosts)
        {
            List<Host> aerospikeHosts = new List<Host> { new Host(primaryHost, port) };
            secondaryHosts.ForEach(host =>
                                          aerospikeHosts.Add(new Host(host, port)));
            return aerospikeHosts.ToArray();
        }

       
    }
}
