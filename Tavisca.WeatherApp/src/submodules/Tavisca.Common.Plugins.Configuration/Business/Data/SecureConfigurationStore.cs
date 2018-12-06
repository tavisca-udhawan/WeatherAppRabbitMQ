using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Tavisca.Platform.Common.Configurations;

namespace Tavisca.Common.Plugins.Configuration
{
    public class SecureConfigurationStore : IConfigurationStore
    {
        private IConfigurationStore _remoteConfigurationStore;
        private ISensitiveDataProvider _sensitiveDataProvider;

        public SecureConfigurationStore(IConfigurationStore remoteConfigurationStore, ISensitiveDataProvider sensitiveDataProvider)
        {            
            _sensitiveDataProvider = sensitiveDataProvider;
            _remoteConfigurationStore = remoteConfigurationStore;
        }

        public string Get(string scope, string application, string section, string key)
        {
            var configurationValue = _remoteConfigurationStore.Get(scope, application, section, key);

            var waitHandle = new ManualResetEvent(false);
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    configurationValue = await ReplaceSensitiveData(configurationValue);
                }
                finally
                {
                    waitHandle.Set();
                }
            });
            waitHandle.WaitOne();

            return configurationValue;
        }

        public async Task<Dictionary<string, string>> GetAllAsync()
        {
            var configurationValues = await _remoteConfigurationStore.GetAllAsync();
            var values = new Dictionary<string, string>();

            foreach(var configurationValue in configurationValues)
            {
                values.Add(configurationValue.Key, await ReplaceSensitiveData(configurationValue.Value));
            }

            return values;
        }

        public async Task<string> GetAsync(string scope, string application, string section, string key)
        {
            var value = await _remoteConfigurationStore.GetAsync(scope, application, section, key);
            return await ReplaceSensitiveData(value);
        }

        public async Task<ConfigurationStoreConnectionStatus> HealthCheckAsync()
        {
            return await _remoteConfigurationStore.HealthCheckAsync();
        }

        private async Task<string> ReplaceSensitiveData(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var pattern = @"\[#([a-zA-Z0-9._-]+)#\]";

            var keyMatches = Regex.Matches(value, pattern);

            if (keyMatches == null || keyMatches.Count == 0)
                return value;

            var keys = new List<string>();

            for (var i = 0; i < keyMatches.Count; i++)
            {
                var match = keyMatches[i];
                keys.Add(match.Groups[1].Value);
            }

            if (_sensitiveDataProvider != null)
            {
                var keyValuePair = await _sensitiveDataProvider.GetValuesAsync(keys.Distinct().ToList(), true);

                if (keyValuePair != null && keyValuePair.Count > 0)
                {
                    foreach (var keyValue in keyValuePair)
                    {
                        value = value.Replace($"[#{keyValue.Key}#]", keyValue.Value);
                    }
                }
            }

            return value;
        }
    }
}