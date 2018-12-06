using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Specialized;
using Tavisca.Platform.Common.Configurations;

namespace Tavisca.Common.Plugins.Configuration
{
    public class ConfigurationProvider : IConfigurationProvider
    {
        private readonly string _applicationName;
        private readonly IConfigurationStore _configurationStore;
        private readonly IJsonSerializer _jsonSerializer;

        public ConfigurationProvider(string applicationName) : this(Defaults.DefaultProvider, Defaults.DefaultSerializer, applicationName, Defaults.DefaultSensitiveDataProvider)
        {
        }

        public ConfigurationProvider(string applicationName, ConfigurationBuilder configurationBuilder)
        {
            StringValidator.ValidateIsNullOrEmpty(applicationName, nameof(applicationName));
            configurationBuilder.Apply();

            _configurationStore = new CachedConfigurationStore(Defaults.DefaultProvider, Defaults.DefaultSensitiveDataProvider);
            _jsonSerializer = Defaults.DefaultSerializer;
            _applicationName = applicationName;
        }

        public ConfigurationProvider(IConfigurationStore remoteConfigurationStore, IJsonSerializer serializer, string applicationName, ISensitiveDataProvider sensitiveDataProvider)
        {
            StringValidator.ValidateIsNullOrEmpty(applicationName, nameof(applicationName));

            if (remoteConfigurationStore != null && serializer != null)
            {
                _configurationStore = new CachedConfigurationStore(remoteConfigurationStore, sensitiveDataProvider);
                _jsonSerializer = serializer;
                _applicationName = applicationName;
                //bus.Register("config-update", new ConfigurationObserver());
            }
            else
            {
                throw Errors.ClientSide.NoConfigurationProviderOrSerializer();
            }
        }

        #region AsyncMethod

        #region Global

        public async Task<string> GetGlobalConfigurationAsStringAsync(string section, string key)
        {
            RequestValidator.ValidateGlobal(section, key);

            var result = await GetGlobalDataAsync(_applicationName, section, key);

            return result.GetAsString();
        }

        public async Task<NameValueCollection> GetGlobalConfigurationAsNameValueCollectionAsync(string section, string key)
        {
            RequestValidator.ValidateGlobal(section, key);

            var result = await GetGlobalDataAsync(_applicationName, section, key);

            return result.GetAsNameValueCollcetion();
        }

        public async Task<T> GetGlobalConfigurationAsync<T>(string section, string key)
        {
            RequestValidator.ValidateGlobal(section, key);

            var result = await GetGlobalDataAsync(_applicationName, section, key);

            return result.GetAs<T>();
        }


        public async Task<string> GetGlobalConfigurationAsStringAsync(string application, string section, string key)
        {
            RequestValidator.ValidateGlobal(application, section, key);

            var result = await GetGlobalDataAsync(application, section, key);

            return result.GetAsString();
        }


        public async Task<NameValueCollection> GetGlobalConfigurationAsNameValueCollectionAsync(string application, string section, string key)
        {
            RequestValidator.ValidateGlobal(application, section, key);

            var result = await GetGlobalDataAsync(application, section, key);

            return result.GetAsNameValueCollcetion();
        }


        public async Task<T> GetGlobalConfigurationAsync<T>(string application, string section, string key)
        {
            RequestValidator.ValidateGlobal(application, section, key);

            var result = await GetGlobalDataAsync(application, section, key);

            return result.GetAs<T>();
        }
        #endregion

        #region Tenant
        public async Task<string> GetTenantConfigurationAsStringAsync(string tenantId, string section, string key)
        {
            RequestValidator.ValidateTenant(tenantId, section, key);

            var result = await GetTenantDataAsync(tenantId, _applicationName, section, key);

            return result.GetAsString();
        }

        public async Task<NameValueCollection> GetTenantConfigurationAsNameValueCollectionAsync(string tenantId, string section, string key)
        {
            RequestValidator.ValidateTenant(tenantId, section, key);

            var result = await GetTenantDataAsync(tenantId, _applicationName, section, key);

            return result.GetAsNameValueCollcetion();
        }

        public async Task<T> GetTenantConfigurationAsync<T>(string tenantId, string section, string key)
        {
            RequestValidator.ValidateTenant(tenantId, section, key);

            var result = await GetTenantDataAsync(tenantId, _applicationName, section, key);

            return result.GetAs<T>();
        }


        public async Task<string> GetTenantConfigurationAsStringAsync(string tenantId, string application, string section, string key)
        {
            RequestValidator.ValidateTenant(tenantId, application, section, key);

            var result = await GetTenantDataAsync(tenantId, application, section, key);

            return result.GetAsString();
        }


        public async Task<NameValueCollection> GetTenantConfigurationAsNameValueCollectionAsync(string tenantId, string application, string section, string key)
        {
            RequestValidator.ValidateTenant(tenantId, application, section, key);

            var result = await GetTenantDataAsync(tenantId, application, section, key);

            return result.GetAsNameValueCollcetion();
        }


        public async Task<T> GetTenantConfigurationAsync<T>(string tenantId, string application, string section, string key)
        {
            RequestValidator.ValidateTenant(tenantId, application, section, key);

            var result = await GetTenantDataAsync(tenantId, application, section, key);

            return result.GetAs<T>();
        }
        #endregion

        private async Task<IConfigurationValue> GetTenantDataAsync(string tenantId, string applicationName, string sectionName, string key)
        {

            var value = await _configurationStore.GetAsync(tenantId, applicationName, sectionName, key);

            if (string.IsNullOrEmpty(value))
                value = await _configurationStore.GetAsync(Constants.Default, applicationName, sectionName, key);

            return new ConfigurationValue(value, _jsonSerializer);
        }

        private async Task<IConfigurationValue> GetGlobalDataAsync(string applicatioName, string sectionName, string key)
        {

            var value = await _configurationStore.GetAsync(Constants.Global, applicatioName, sectionName, key);

            return new ConfigurationValue(value, _jsonSerializer);
        }

        #endregion

        #region SyncMethod

        #region Global
        public string GetGlobalConfigurationAsString(string section, string key)
        {
            RequestValidator.ValidateGlobal(section, key);

            var result = GetGlobalData(_applicationName, section, key);

            return result.GetAsString();
        }

        public NameValueCollection GetGlobalConfigurationAsNameValueCollection(string section, string key)
        {
            RequestValidator.ValidateGlobal(section, key);

            var result = GetGlobalData(_applicationName, section, key);

            return result.GetAsNameValueCollcetion();
        }

        public T GetGlobalConfiguration<T>(string section, string key)
        {
            RequestValidator.ValidateGlobal(section, key);

            var result = GetGlobalData(_applicationName, section, key);

            return result.GetAs<T>();
        }

        public string GetGlobalConfigurationAsString(string applicationName, string section, string key)
        {
            RequestValidator.ValidateGlobal(applicationName, section, key);

            var result = GetGlobalData(applicationName, section, key);

            return result.GetAsString();
        }

        public NameValueCollection GetGlobalConfigurationAsNameValueCollection(string applicationName, string section, string key)
        {
            RequestValidator.ValidateGlobal(applicationName, section, key);

            var result = GetGlobalData(applicationName, section, key);

            return result.GetAsNameValueCollcetion();
        }

        public T GetGlobalConfiguration<T>(string applicationName, string section, string key)
        {
            RequestValidator.ValidateGlobal(applicationName, section, key);

            var result = GetGlobalData(applicationName, section, key);

            return result.GetAs<T>();
        }
        #endregion

        #region Tenant
        public string GetTenantConfigurationAsString(string tenantId, string section, string key)
        {
            RequestValidator.ValidateTenant(tenantId, section, key);

            var result = GetTenantData(tenantId, _applicationName, section, key);

            return result.GetAsString();
        }

        public NameValueCollection GetTenantConfigurationAsNameValueCollection(string tenantId, string section, string key)
        {
            RequestValidator.ValidateTenant(tenantId, section, key);

            var result = GetTenantData(tenantId, _applicationName, section, key);

            return result.GetAsNameValueCollcetion();
        }

        public T GetTenantConfiguration<T>(string tenantId, string section, string key)
        {
            RequestValidator.ValidateTenant(tenantId, section, key);

            var result = GetTenantData(tenantId, _applicationName, section, key);

            return result.GetAs<T>();
        }

        public string GetTenantConfigurationAsString(string tenantId, string applicationName, string section, string key)
        {
            RequestValidator.ValidateTenant(tenantId, applicationName, section, key);

            var result = GetTenantData(tenantId, applicationName, section, key);

            return result.GetAsString();
        }

        public NameValueCollection GetTenantConfigurationAsNameValueCollection(string tenantId, string applicationName, string section, string key)
        {
            RequestValidator.ValidateTenant(tenantId, applicationName, section, key);

            var result = GetTenantData(tenantId, applicationName, section, key);

            return result.GetAsNameValueCollcetion();
        }

        public T GetTenantConfiguration<T>(string tenantId, string applicationName, string section, string key)
        {
            RequestValidator.ValidateTenant(tenantId, applicationName, section, key);

            var result = GetTenantData(tenantId, applicationName, section, key);

            return result.GetAs<T>();
        }

        #endregion

        private IConfigurationValue GetTenantData(string tenantId, string applicationName, string sectionName, string key)
        {

            var value = _configurationStore.Get(tenantId, applicationName, sectionName, key);

            if (string.IsNullOrEmpty(value))
                value = _configurationStore.Get(Constants.Default, applicationName, sectionName, key);

            return new ConfigurationValue(value, _jsonSerializer);
        }

        private IConfigurationValue GetGlobalData(string applicatioName, string sectionName, string key)
        {

            var value = _configurationStore.Get(Constants.Global, applicatioName, sectionName, key);

            return new ConfigurationValue(value, _jsonSerializer);
        }

        public async Task<ConfigurationProviderConnectionStatus> HealthCheckAsync()
        {
            var connectionStatus = await _configurationStore.HealthCheckAsync();

            return connectionStatus == ConfigurationStoreConnectionStatus.Connected
                            ? ConfigurationProviderConnectionStatus.Connected
                            : ConfigurationProviderConnectionStatus.Disconnected;
        }


        #endregion


        #region featureFlagSetting
        #region async methods
        private async Task<IConfigurationValue> GetFeatureFlagSettingsAsync(string tenantId, string applicationName, string sectionName, string key)
        {

            var value = await _configurationStore.GetAsync(tenantId, applicationName, sectionName, key);

            if (string.IsNullOrEmpty(value))
                value = await _configurationStore.GetAsync(Constants.Default, applicationName, sectionName, key);
            if (string.IsNullOrEmpty(value))
                value = await _configurationStore.GetAsync(Constants.Global, applicationName, sectionName, key);

            return new ConfigurationValue(value, _jsonSerializer);
        }

        public async Task<string> GetFeatureFlagSettingsAsStringAsync(string tenantId, string key)
        {
            RequestValidator.ValidateGlobal(Constants.Section.FeatureFlagSettings, key);

            var result = await GetFeatureFlagSettingsAsync(tenantId, _applicationName, Constants.Section.FeatureFlagSettings, key);

            return result.GetAsString();
        }

        public async Task<NameValueCollection> GetFeatureFlagSettingsAsNameValueCollectionAsync(string tenantId, string key)
        {
            RequestValidator.ValidateGlobal(Constants.Section.FeatureFlagSettings, key);

            var result = await GetFeatureFlagSettingsAsync(tenantId, _applicationName, Constants.Section.FeatureFlagSettings, key);

            return result.GetAsNameValueCollcetion();
        }

        public async Task<T> GetFeatureFlagSettingsAsObjectAsync<T>(string tenantId, string key)
        {
            RequestValidator.ValidateGlobal(Constants.Section.FeatureFlagSettings, key);

            var result = await GetFeatureFlagSettingsAsync(tenantId, _applicationName, Constants.Section.FeatureFlagSettings, key);

            return result.GetAs<T>();
        }

        public async Task<string> GetFeatureFlagSettingsAsStringAsync(string tenantId, string applicationName, string key)
        {
            RequestValidator.ValidateGlobal(applicationName, Constants.Section.FeatureFlagSettings, key);

            var result = await GetFeatureFlagSettingsAsync(tenantId, applicationName, Constants.Section.FeatureFlagSettings, key);

            return result.GetAsString();
        }

        public async Task<NameValueCollection> GetFeatureFlagSettingsAsNameValueCollectionAsync(string tenantId, string applicationName, string key)
        {
            RequestValidator.ValidateGlobal(applicationName, Constants.Section.FeatureFlagSettings, key);

            var result = await GetFeatureFlagSettingsAsync(tenantId, applicationName, Constants.Section.FeatureFlagSettings, key);

            return result.GetAsNameValueCollcetion();
        }

        public async Task<T> GetFeatureFlagSettingsAsObjectAsync<T>(string tenantId, string applicationName, string key)
        {
            RequestValidator.ValidateGlobal(applicationName, Constants.Section.FeatureFlagSettings, key);

            var result = await GetFeatureFlagSettingsAsync(tenantId, applicationName, Constants.Section.FeatureFlagSettings, key);

            return result.GetAs<T>();
        }
        #endregion

        #region sync methods

        private IConfigurationValue GetFeatureFlagSettings(string tenantId, string applicationName, string sectionName, string key)
        {

            var value = _configurationStore.Get(tenantId, applicationName, sectionName, key);

            if (string.IsNullOrEmpty(value))
                value = _configurationStore.Get(Constants.Default, applicationName, sectionName, key);
            if (string.IsNullOrEmpty(value))
                value = _configurationStore.Get(Constants.Global, applicationName, sectionName, key);

            return new ConfigurationValue(value, _jsonSerializer);
        }
        public string GetFeatureFlagSettingsAsString(string tenantId, string key)
        {
            RequestValidator.ValidateGlobal(Constants.Section.FeatureFlagSettings, key);

            var result = GetFeatureFlagSettings(tenantId, _applicationName, Constants.Section.FeatureFlagSettings, key);

            return result.GetAsString();
        }

        public NameValueCollection GetFeatureFlagSettingsAsNameValueCollection(string tenantId, string key)
        {
            RequestValidator.ValidateGlobal(Constants.Section.FeatureFlagSettings, key);

            var result = GetFeatureFlagSettings(tenantId, _applicationName, Constants.Section.FeatureFlagSettings, key);

            return result.GetAsNameValueCollcetion();
        }

        public T GetFeatureFlagSettingsAsObject<T>(string tenantId, string key)
        {
            RequestValidator.ValidateGlobal(Constants.Section.FeatureFlagSettings, key);

            var result = GetFeatureFlagSettings(tenantId, _applicationName, Constants.Section.FeatureFlagSettings, key);

            return result.GetAs<T>();
        }

        public string GetFeatureFlagSettingsAsString(string tenantId, string applicationName, string key)
        {
            RequestValidator.ValidateGlobal(applicationName, Constants.Section.FeatureFlagSettings, key);

            var result = GetFeatureFlagSettings(tenantId, applicationName, Constants.Section.FeatureFlagSettings, key);

            return result.GetAsString();
        }

        public NameValueCollection GetFeatureFlagSettingsAsNameValueCollection(string tenantId, string applicationName, string key)
        {
            RequestValidator.ValidateGlobal(applicationName, Constants.Section.FeatureFlagSettings, key);

            var result = GetFeatureFlagSettings(tenantId, applicationName, Constants.Section.FeatureFlagSettings, key);

            return result.GetAsNameValueCollcetion();
        }

        public T GetFeatureFlagSettingsAsObject<T>(string tenantId, string applicationName, string key)
        {
            RequestValidator.ValidateGlobal(applicationName, Constants.Section.FeatureFlagSettings, key);

            var result = GetFeatureFlagSettings(tenantId, applicationName, Constants.Section.FeatureFlagSettings, key);

            return result.GetAs<T>();
        } 
        #endregion
        #endregion
    }

    internal static class Defaults
    {
        public static IConfigurationStore DefaultProvider = new ConsulConfigurationStore();

        public static IJsonSerializer DefaultSerializer = new JsonSerializer();

        public static ISensitiveDataProvider DefaultSensitiveDataProvider = null;
    }

    public class ConfigurationBuilder
    {
        private readonly List<Action> _actions = new List<Action>();

        public ConfigurationBuilder WithRemoteStore(IConfigurationStore store)
        {
            _actions.Add(() => Defaults.DefaultProvider = store ?? Defaults.DefaultProvider);
            return this;
        }

        public ConfigurationBuilder WithSerializer(IJsonSerializer serializer)
        {
            _actions.Add(() => Defaults.DefaultSerializer = serializer ?? Defaults.DefaultSerializer);
            return this;
        }

        public ConfigurationBuilder WithSensitiveDataProvider(ISensitiveDataProvider provider)
        {
            _actions.Add(() => Defaults.DefaultSensitiveDataProvider = provider ?? Defaults.DefaultSensitiveDataProvider);
            return this;
        }

        public void Apply()
        {
            _actions.ForEach(action => action());
        }
    }
}
