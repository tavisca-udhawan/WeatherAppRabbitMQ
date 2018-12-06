using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.Configuration;
using Tavisca.Platform.Common.ApplicationEventBus;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.FileStore;

namespace Tavisca.Common.Plugins.Aws.S3
{
    public class S3StoreFactory : ApplicationEventObserver, IFileStoreFactory
    {
        private static readonly AwaitableLock _lock = new AwaitableLock();
        private static readonly AwaitableLock _listLock = new AwaitableLock();
        private static ConcurrentDictionary<string, Tuple<IFileStore, List<string>>> _clients = new ConcurrentDictionary<string, Tuple<IFileStore, List<string>>>();
        private static ConcurrentDictionary<string, IFileStore> _tenantSpecificClients = new ConcurrentDictionary<string, IFileStore>();
        private string _platformTenant = "0";
        private static int isApplicationEventRegistered = 0;
        private static object _syncLock = new object();

        private readonly IConfigurationProvider _config;
        public S3StoreFactory(IConfigurationProvider configuration) : this(configuration, new InstanceEventBus())
        {
        }

        private S3StoreFactory(IConfigurationProvider configuration, IApplicationEventBus applicationEventBus)
        {
            _config = configuration;
            if (Interlocked.Exchange(ref isApplicationEventRegistered, 1) == 0)
                applicationEventBus.Register("in-memory-consul-cache-refresh", this);
        }

        public async Task<IFileStore> GetTenantSpecificClientAsync(string tenantId)
        {
            if (_tenantSpecificClients == null)
                _tenantSpecificClients = new ConcurrentDictionary<string, IFileStore>();

            if (_tenantSpecificClients.ContainsKey(tenantId))
                return _tenantSpecificClients[tenantId];
            else
                return await GetClientAsync(tenantId, await GetTenantSettingsAsync(tenantId));
        }

        public async Task<IFileStore> GetGlobalClientAsync()
        {
            if (_tenantSpecificClients == null)
                _tenantSpecificClients = new ConcurrentDictionary<string, IFileStore>();

            if (_tenantSpecificClients.ContainsKey(_platformTenant))
                return _tenantSpecificClients[_platformTenant];
            else
                return await GetClientAsync(_platformTenant, await GetGlobalSettingsAsync());
        }

        public IFileStore GetTenantSpecificClient(string tenantId)
        {
            if (_tenantSpecificClients == null)
                _tenantSpecificClients = new ConcurrentDictionary<string, IFileStore>();

            if (_tenantSpecificClients.ContainsKey(tenantId))
                return _tenantSpecificClients[tenantId];
            else
                return GetClient(tenantId, GetTenantSettings(tenantId));
        }

        public IFileStore GetGlobalClient()
        {
            if (_tenantSpecificClients == null)
                _tenantSpecificClients = new ConcurrentDictionary<string, IFileStore>();

            if (_tenantSpecificClients.ContainsKey(_platformTenant))
                return _tenantSpecificClients[_platformTenant];
            else
                return GetClient(_platformTenant, GetGlobalSettings());
        }

        public override async void Process(ApplicationEvent msg)
        {
            using (await _lock.LockAsync())
            {
                var clients = new Dictionary<string, Tuple<IFileStore, List<string>>>(_clients);
                foreach (var client in clients)
                {
                    var oldSettingsSignature = client.Key;
                    var tenants = new List<string>(client.Value.Item2);

                    foreach (var tenant in tenants)
                    {
                        S3Settings updatedSettings;
                        if (tenant.Equals(_platformTenant, StringComparison.OrdinalIgnoreCase))
                            updatedSettings = await GetGlobalSettingsAsync();
                        else
                            updatedSettings = await GetTenantSettingsAsync(tenant);

                        if (oldSettingsSignature.Equals(updatedSettings.Signature) == false) //Settings are updated,client needs to be updated
                        {
                            var oldClientWithUpdatedSettings = _clients.FirstOrDefault(x => x.Key.Equals(updatedSettings.Signature));
                            if (oldClientWithUpdatedSettings.Key != null && oldClientWithUpdatedSettings.Value != null && oldClientWithUpdatedSettings.Value.Item2 != null)
                            {
                                using (await _listLock.LockAsync())
                                {
                                    oldClientWithUpdatedSettings.Value.Item2.Add(tenant);
                                    _clients[oldSettingsSignature].Item2.Remove(tenant);
                                    _tenantSpecificClients[tenant] = oldClientWithUpdatedSettings.Value.Item1;
                                }
                            }
                            else //No client exists with specified settings,create new client
                            {
                                using (await _listLock.LockAsync())
                                {
                                    var newClient = CreateClient(updatedSettings); //Create new client
                                    _clients.TryAdd(updatedSettings.Signature, new Tuple<IFileStore, List<string>>(newClient, new List<string>() { tenant }));
                                    _clients[oldSettingsSignature].Item2.Remove(tenant);
                                    _tenantSpecificClients[tenant] = newClient;
                                }
                            }
                        }
                    }
                }
                //Dispose unused clients
                ClearUnusedClients(_clients);
            }
        }

        private IFileStore GetClient(string tenantId, S3Settings settings)
        {
            lock (_syncLock)
            {
                return GetS3Client(tenantId, settings);
            }
        }

        private IFileStore GetS3Client(string tenantId, S3Settings settings)
        {
            var clientData = _clients.FirstOrDefault(x => x.Key.Equals(settings.Signature));

            if (clientData.Key != null && clientData.Value != null && clientData.Value.Item1 != null && clientData.Value.Item2 != null)
            {
                clientData.Value.Item2.Add(tenantId);
                _tenantSpecificClients[tenantId] = clientData.Value.Item1;
                return clientData.Value.Item1;
            }
            else
            {
                var client = CreateClient(settings);
                _clients.TryAdd(settings.Signature, new Tuple<IFileStore, List<string>>(client, new List<string>() { tenantId }));
                _tenantSpecificClients[tenantId] = client;
                return client;
            }
        }

        private S3Settings GetGlobalSettings()
        {
            var nvc = _config.GetGlobalConfigurationAsNameValueCollection(Constants.DataEncryptionSection, Constants.S3Settings);

            var settings = S3Settings.Load(nvc ?? new NameValueCollection());

            return settings;
        }

        private S3Settings GetTenantSettings(string tenantId)
        {
            var nvc = _config.GetTenantConfigurationAsNameValueCollection(tenantId, Constants.DataEncryptionSection, Constants.S3Settings);

            var settings = S3Settings.Load(nvc ?? new NameValueCollection());

            return settings;
        }

        private async Task<IFileStore> GetClientAsync(string tenantId, S3Settings settings)
        {
            using (await _listLock.LockAsync())
            {
                return GetS3Client(tenantId, settings);
            }
        }
        private async Task<S3Settings> GetGlobalSettingsAsync()
        {
            var nvc = await _config.GetGlobalConfigurationAsNameValueCollectionAsync(Constants.DataEncryptionSection, Constants.S3Settings);

            var settings = S3Settings.Load(nvc ?? new NameValueCollection());

            return settings;
        }


        private async Task<S3Settings> GetTenantSettingsAsync(string tenantId)
        {
            var nvc = await _config.GetTenantConfigurationAsNameValueCollectionAsync(tenantId, Constants.DataEncryptionSection, Constants.S3Settings);

            var settings = S3Settings.Load(nvc ?? new NameValueCollection());

            return settings;
        }

        private IFileStore CreateClient(S3Settings settings)
        {
            var region = RegionEndpoint.GetBySystemName(settings.Region);
            if (settings.HasKeys == true)
            {
                var credentials = new BasicAWSCredentials(settings.AccessKey, settings.SecretKey);
                return new S3Store(new AmazonS3Client(credentials, region));
            }
            else
            {
                return new S3Store(new AmazonS3Client(region));
            }
        }

        private async void ClearUnusedClients(ConcurrentDictionary<string, Tuple<IFileStore, List<string>>> _clients)
        {
            var clientsToBeDisposed = new List<IFileStore>();
            var clients = new Dictionary<string, Tuple<IFileStore, List<string>>>(_clients);
            using (await _listLock.LockAsync())
            {
                foreach (var client in clients)
                {
                    if (client.Value.Item2.Count == 0)
                    {
                        var oldClient = client.Value.Item1;
                        if (oldClient != null)
                        {
                            clientsToBeDisposed.Add(oldClient);
                            Tuple<IFileStore, List<string>> removed;
                            _clients.TryRemove(client.Key, out removed);
                        }
                    }
                }

                if (clientsToBeDisposed != null && clientsToBeDisposed.Count > 0)
                {
                    await Task.Delay(10000)
                    .ContinueWith((x) =>
                    {
                        ClearClients(clientsToBeDisposed);
                    });
                }
            }
        }

        private void ClearClients(List<IFileStore> clientsToBeDisposed)
        {
            foreach (var client in clientsToBeDisposed)
                client.Dispose();
        }
    }
}
