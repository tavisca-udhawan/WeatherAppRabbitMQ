using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Configurations;

namespace Tavisca.Common.Plugins.Configuration
{
    public class CachedConfigurationStore : IConfigurationStore
    {
        private readonly IConfigurationStore _remoteConfigurationStore;

        public CachedConfigurationStore()
        {
            _remoteConfigurationStore = new SecureConfigurationStore(new ConsulConfigurationStore(), null);
        }

        public CachedConfigurationStore(IConfigurationStore remoteConfigurationStore, ISensitiveDataProvider sensitiveDataProvider)
        {
            _remoteConfigurationStore = new SecureConfigurationStore(remoteConfigurationStore, sensitiveDataProvider);
        }

        private static readonly ConcurrentDictionary<string, string> _kvStorage = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public string Get(string scope, string application, string section, string key)
        {
            var cacheKey = KeyMaker.ConstructKey(scope, application, section, key);
            var result = LocalConfigurationRepository.IsKeyPresent(cacheKey)
                            ? LocalConfigurationRepository.Get(cacheKey)
                            : GetFromRemote(scope, application, section, key);
            return result;
        }
        public async Task<string> GetAsync(string scope, string application, string section, string key)
        {
            var cacheKey = KeyMaker.ConstructKey(scope, application, section, key);
            var result = LocalConfigurationRepository.IsKeyPresent(cacheKey)
                            ? await GetFromLocalAsync(cacheKey)
                            : await GetFromRemoteAsync(scope, application, section, key);
            return result;
        }

        private string GetFromRemote(string scope, string application, string section, string key)
        {
            var value = _remoteConfigurationStore.Get(scope, application, section, key);
            var cacheKey = KeyMaker.ConstructKey(scope, application, section, key);
            LocalConfigurationRepository.Update(cacheKey, value);
            return value;
        }

        private async Task<string> GetFromRemoteAsync(string scope, string application, string section, string key)
        {
            var value = await _remoteConfigurationStore.GetAsync(scope, application, section, key);
            var cacheKey = KeyMaker.ConstructKey(scope, application, section, key);
            LocalConfigurationRepository.Update(cacheKey, value);
            return value;
        }
        private Task<string> GetFromLocalAsync(string key)
        {
            string value = LocalConfigurationRepository.Get(key);
            return Task.FromResult<string>(value);
        }

        public async Task<Dictionary<string, string>> GetAllAsync()
        {
            return await _remoteConfigurationStore.GetAllAsync();
        }

        public async Task<ConfigurationStoreConnectionStatus> HealthCheckAsync()
        {
            return await _remoteConfigurationStore.HealthCheckAsync();
        }
    }
}
