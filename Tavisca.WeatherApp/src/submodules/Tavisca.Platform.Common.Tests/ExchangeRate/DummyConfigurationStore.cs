using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.ExchangeRate.Entities;
using Tavisca.Platform.Common.Configurations;

namespace Tavisca.Common.Tests
{
    public class DummyConfigurationStore : IConfigurationProvider
    {
        internal NameValueCollection Configuration;

        public NameValueCollection GetFeatureFlagSettingsAsNameValueCollection(string tenantId, string key)
        {
            throw new NotImplementedException();
        }

        public NameValueCollection GetFeatureFlagSettingsAsNameValueCollection(string tenantId, string applicationName, string key)
        {
            throw new NotImplementedException();
        }

        public Task<NameValueCollection> GetFeatureFlagSettingsAsNameValueCollectionAsync(string tenantId, string key)
        {
            throw new NotImplementedException();
        }

        public Task<NameValueCollection> GetFeatureFlagSettingsAsNameValueCollectionAsync(string tenantId, string applicationName, string key)
        {
            throw new NotImplementedException();
        }

        public T GetFeatureFlagSettingsAsObject<T>(string tenantId, string key)
        {
            throw new NotImplementedException();
        }

        public T GetFeatureFlagSettingsAsObject<T>(string tenantId, string applicationName, string key)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetFeatureFlagSettingsAsObjectAsync<T>(string tenantId, string key)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetFeatureFlagSettingsAsObjectAsync<T>(string tenantId, string applicationName, string key)
        {
            throw new NotImplementedException();
        }

        public string GetFeatureFlagSettingsAsString(string tenantId, string key)
        {
            throw new NotImplementedException();
        }

        public string GetFeatureFlagSettingsAsString(string tenantId, string applicationName, string key)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetFeatureFlagSettingsAsStringAsync(string tenantId, string key)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetFeatureFlagSettingsAsStringAsync(string tenantId, string applicationName, string key)
        {
            throw new NotImplementedException();
        }

        public T GetGlobalConfiguration<T>(string section, string key)
        {
            throw new NotImplementedException();
        }

        public T GetGlobalConfiguration<T>(string applicationName, string section, string key)
        {
            throw new NotImplementedException();
        }

        public NameValueCollection GetGlobalConfigurationAsNameValueCollection(string section, string key)
        {
            throw new NotImplementedException();
        }

        public NameValueCollection GetGlobalConfigurationAsNameValueCollection(string applicationName, string section, string key)
        {
            throw new NotImplementedException();
        }

        public async Task<NameValueCollection> GetGlobalConfigurationAsNameValueCollectionAsync(string section, string key)
        {
            return Configuration;
        }

        public Task<NameValueCollection> GetGlobalConfigurationAsNameValueCollectionAsync(string applicationName, string section, string key)
        {
            throw new NotImplementedException();
        }

        public string GetGlobalConfigurationAsString(string section, string key)
        {
            throw new NotImplementedException();
        }

        public string GetGlobalConfigurationAsString(string applicationName, string section, string key)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetGlobalConfigurationAsStringAsync(string section, string key)
        {
            if (string.Equals(section, Constants.SectionName, StringComparison.InvariantCultureIgnoreCase)
                &&
                string.Equals(key, Constants.NumberOfSlots, StringComparison.InvariantCultureIgnoreCase))
                return Configuration[Constants.NumberOfSlots];



            return string.Empty;
        }

        public async Task<string> GetGlobalConfigurationAsStringAsync(string applicationName, string section, string key)
        {
            if (Configuration == null)
                return string.Empty;

            if (string.Equals(key, Constants.DynamoDBServiceUrl))
                return Configuration[Constants.DynamoDBServiceUrl];

            if (string.Equals(key, Constants.AWSAccessKey))
                return Configuration[Constants.AWSAccessKey];

            if (string.Equals(key, Constants.AWSSecretKey))
                return Configuration[Constants.AWSSecretKey];

            if (string.Equals(key, Constants.NumberOfSlots))
                return Configuration[Constants.NumberOfSlots];

            return string.Empty;


        }

        public Task<T> GetGlobalConfigurationAsync<T>(string section, string key)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetGlobalConfigurationAsync<T>(string applicationName, string section, string key)
        {
            throw new NotImplementedException();
        }

        
        public T GetTenantConfiguration<T>(string tenantId, string section, string key)
        {
            throw new NotImplementedException();
        }

        public T GetTenantConfiguration<T>(string tenantId, string applicationName, string section, string key)
        {
            throw new NotImplementedException();
        }

        public NameValueCollection GetTenantConfigurationAsNameValueCollection(string tenantId, string section, string key)
        {
            throw new NotImplementedException();
        }

        public NameValueCollection GetTenantConfigurationAsNameValueCollection(string tenantId, string applicationName, string section, string key)
        {
            throw new NotImplementedException();
        }

        public Task<NameValueCollection> GetTenantConfigurationAsNameValueCollectionAsync(string tenantId, string section, string key)
        {
            throw new NotImplementedException();
        }

        public Task<NameValueCollection> GetTenantConfigurationAsNameValueCollectionAsync(string tenantId, string applicationName, string section, string key)
        {
            throw new NotImplementedException();
        }

        public string GetTenantConfigurationAsString(string tenantId, string section, string key)
        {
            throw new NotImplementedException();
        }

        public string GetTenantConfigurationAsString(string tenantId, string applicationName, string section, string key)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetTenantConfigurationAsStringAsync(string tenantId, string section, string key)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetTenantConfigurationAsStringAsync(string tenantId, string applicationName, string section, string key)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetTenantConfigurationAsync<T>(string tenantId, string section, string key)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetTenantConfigurationAsync<T>(string tenantId, string applicationName, string section, string key)
        {
            throw new NotImplementedException();
        }

        

        public Task<ConfigurationProviderConnectionStatus> HealthCheckAsync()
        {
            throw new NotImplementedException();
        }
    }
}
