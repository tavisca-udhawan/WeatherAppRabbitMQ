using System.Threading.Tasks;
using Amazon.KeyManagementService;
using Amazon;
using Amazon.Runtime;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Tavisca.Platform.Common.ApplicationEventBus;
using System;
using Tavisca.Platform.Common.Configurations;
using System.Linq;
using System.Collections.Specialized;
using System.Threading;
using Tavisca.Common.Plugins.Configuration;

namespace Tavisca.Common.Plugins.Aws.KMS
{
    public class KmsClientFactory : ApplicationEventObserver, IKmsClientFactory
    {
        private static readonly AwaitableLock _lock = new AwaitableLock();
        private static readonly AwaitableLock _listLock = new AwaitableLock();
        private static ConcurrentDictionary<string, Tuple<IAmazonKeyManagementService, List<string>>> _clients = new ConcurrentDictionary<string, Tuple<IAmazonKeyManagementService, List<string>>>();
        private static ConcurrentDictionary<string, IAmazonKeyManagementService> _tenantSpecificClients = new ConcurrentDictionary<string, IAmazonKeyManagementService>();
        private string _platformTenant = "0";
        private static object _syncLock = new object();
        private static int isApplicationEventRegistered = 0;

        private readonly IConfigurationProvider _config;
        public KmsClientFactory(IConfigurationProvider configuration) : this(configuration, new InstanceEventBus())
        {
        }

        private KmsClientFactory(IConfigurationProvider configuration, IApplicationEventBus applicationEventBus)
        {
            _config = configuration;
            if (Interlocked.Exchange(ref isApplicationEventRegistered, 1) == 0)
                applicationEventBus.Register("in-memory-consul-cache-refresh", this);
        }

        public async Task<IAmazonKeyManagementService> GetClientAsync(KmsSettings settings)
        {
            using (await _listLock.LockAsync())
            {
                return GetKmsClient(settings);
            }
        }

        public async Task<IAmazonKeyManagementService> GetTenantSpecificClientAsync(string tenantId)
        {
            if (_tenantSpecificClients == null)
                _tenantSpecificClients = new ConcurrentDictionary<string, IAmazonKeyManagementService>();

            if (_tenantSpecificClients.ContainsKey(tenantId))
                return _tenantSpecificClients[tenantId];
            else
                return await GetKmsClientAsync(tenantId, await GetTenantSettingsAsync(tenantId));
        }

        public async Task<IAmazonKeyManagementService> GetGlobalClientAsync()
        {
            if (_tenantSpecificClients == null)
                _tenantSpecificClients = new ConcurrentDictionary<string, IAmazonKeyManagementService>();

            if (_tenantSpecificClients.ContainsKey(_platformTenant))
                return _tenantSpecificClients[_platformTenant];
            else
                return await GetKmsClientAsync(_platformTenant, await GetGlobalSettingsAsync());
        }

                
        public IAmazonKeyManagementService GetClient(KmsSettings settings)
        {
            lock (_syncLock)
            {
                return GetKmsClient(settings);
            }
        }

        private IAmazonKeyManagementService GetKmsClient(KmsSettings settings)
        {
            var clientData = _clients.FirstOrDefault(x => x.Key.Equals(settings.Signature));

            if (clientData.Key != null && clientData.Value != null && clientData.Value.Item1 != null && clientData.Value.Item2 != null)
                return clientData.Value.Item1;
            else
            {
                var client = CreateKmsClient(settings);
                _clients.TryAdd(settings.Signature, new Tuple<IAmazonKeyManagementService, List<string>>(client, new List<string>()));
                return client;
            }
        }

        public IAmazonKeyManagementService GetTenantSpecificClient(string tenantId)
        {
            if (_tenantSpecificClients == null)
                _tenantSpecificClients = new ConcurrentDictionary<string, IAmazonKeyManagementService>();

            if (_tenantSpecificClients.ContainsKey(tenantId))
                return _tenantSpecificClients[tenantId];
            else
                return GetKmsClient(tenantId, GetTenantSettings(tenantId));
        }

        public IAmazonKeyManagementService GetGlobalClient()
        {
            if (_tenantSpecificClients == null)
                _tenantSpecificClients = new ConcurrentDictionary<string, IAmazonKeyManagementService>();

            if (_tenantSpecificClients.ContainsKey(_platformTenant))
                return _tenantSpecificClients[_platformTenant];
            else
                return GetKmsClient(_platformTenant, GetGlobalSettings());
        }

        public override async void Process(ApplicationEvent msg)
        {
            using (await _lock.LockAsync())
            {
                var clients = new Dictionary<string, Tuple<IAmazonKeyManagementService, List<string>>>(_clients);
                foreach (var client in clients)
                {
                    var oldSettingsSignature = client.Key;
                    var tenants = new List<string>(client.Value.Item2);

                    foreach (var tenant in tenants)
                    {
                        KmsSettings updatedSettings;
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
                                    var newClient = CreateKmsClient(updatedSettings); //Create new client
                                    _clients.TryAdd(updatedSettings.Signature, new Tuple<IAmazonKeyManagementService, List<string>>(newClient, new List<string>() { tenant }));
                                    _clients[oldSettingsSignature].Item2.Remove(tenant);
                                    _tenantSpecificClients[tenant] = newClient;
                                }
                            }
                        }
                    }
                }
                //Dispose unused clients
                ClearUnusedClientsAsync(_clients);
            }
        }

        private IAmazonKeyManagementService GetKmsClient(string tenantId, KmsSettings settings)
        {
            lock (_syncLock)
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
                    var client = CreateKmsClient(settings);
                    _clients.TryAdd(settings.Signature, new Tuple<IAmazonKeyManagementService, List<string>>(client, new List<string>() { tenantId }));
                    _tenantSpecificClients[tenantId] = client;
                    return client;
                }
            }
        }

        private KmsSettings GetGlobalSettings()
        {
            var nvc = _config.GetGlobalConfigurationAsNameValueCollection(Constants.DataEncryptionSection, Constants.DataEncryptionKey);

            var settings = KmsSettings.Load(nvc ?? new NameValueCollection());

            return settings;
        }

        private KmsSettings GetTenantSettings(string tenantId)
        {
            var nvc = _config.GetTenantConfigurationAsNameValueCollection(tenantId, Constants.DataEncryptionSection, Constants.DataEncryptionKey);

            var settings = KmsSettings.Load(nvc ?? new NameValueCollection());

            return settings;
        }

        private IAmazonKeyManagementService CreateKmsClient(KmsSettings settings)
        {
            var region = RegionEndpoint.GetBySystemName(settings.Region);
            if (settings.HasKeys == true)
            {
                var credentials = new BasicAWSCredentials(settings.AccessKey, settings.SecretKey);
                return new AmazonKeyManagementServiceClient(credentials, region);
            }
            else
            {
                return new AmazonKeyManagementServiceClient(region);
            }
        }

        private async void ClearUnusedClientsAsync(ConcurrentDictionary<string, Tuple<IAmazonKeyManagementService, List<string>>> _clients)
        {
            var clients = new Dictionary<string, Tuple<IAmazonKeyManagementService, List<string>>>(_clients);
            using (await _listLock.LockAsync())
            {
                foreach (var client in clients)
                {
                    if (client.Value.Item2.Count == 0)
                    {
                        var oldClient = client.Value.Item1;
                        if (oldClient != null)
                        {
                            Tuple<IAmazonKeyManagementService, List<string>> removed;
                            _clients.TryRemove(client.Key, out removed);
                        }
                    }
                }

            }
        }

        private async Task<IAmazonKeyManagementService> GetKmsClientAsync(string tenantId, KmsSettings settings)
        {
            using (await _listLock.LockAsync())
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
                    var client = CreateKmsClient(settings);
                    _clients.TryAdd(settings.Signature, new Tuple<IAmazonKeyManagementService, List<string>>(client, new List<string>() { tenantId }));
                    _tenantSpecificClients[tenantId] = client;
                    return client;
                }
            }
        }

        private async Task<KmsSettings> GetGlobalSettingsAsync()
        {
            var nvc = await _config.GetGlobalConfigurationAsNameValueCollectionAsync(Constants.DataEncryptionSection, Constants.DataEncryptionKey);

            var settings = KmsSettings.Load(nvc ?? new NameValueCollection());

            return settings;
        }

        private async Task<KmsSettings> GetTenantSettingsAsync(string tenantId)
        {
            var nvc = await _config.GetTenantConfigurationAsNameValueCollectionAsync(tenantId, Constants.DataEncryptionSection, Constants.DataEncryptionKey);

            var settings = KmsSettings.Load(nvc ?? new NameValueCollection());

            return settings;
        }

    }
}
