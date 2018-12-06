using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Configurations
{
    public interface IConfigurationProvider
    {
        #region AsyncMethods
        Task<string> GetFeatureFlagSettingsAsStringAsync(string tenantId, string key);
        Task<NameValueCollection> GetFeatureFlagSettingsAsNameValueCollectionAsync(string tenantId, string key);
        Task<T> GetFeatureFlagSettingsAsObjectAsync<T>(string tenantId, string key);

        Task<string> GetFeatureFlagSettingsAsStringAsync(string tenantId, string applicationName, string key);
        Task<NameValueCollection> GetFeatureFlagSettingsAsNameValueCollectionAsync(string tenantId, string applicationName, string key);
        Task<T> GetFeatureFlagSettingsAsObjectAsync<T>(string tenantId, string applicationName, string key);

        Task<string> GetGlobalConfigurationAsStringAsync(string section, string key);
        Task<NameValueCollection> GetGlobalConfigurationAsNameValueCollectionAsync(string section, string key);

        Task<T> GetGlobalConfigurationAsync<T>(string section, string key);
        /// <summary>
        /// applicationName will override applicatioName passed in constructor
        /// </summary>
        /// <param name="applicationName"> </param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<string> GetGlobalConfigurationAsStringAsync(string applicationName, string section, string key);

        /// <summary>
        /// applicationName will override applicatioName passed in constructor
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<NameValueCollection> GetGlobalConfigurationAsNameValueCollectionAsync(string applicationName, string section, string key);

        /// <summary>
        /// applicationName will override applicatioName passed in constructor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="applicationName"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T> GetGlobalConfigurationAsync<T>(string applicationName, string section, string key);

        Task<string> GetTenantConfigurationAsStringAsync(string tenantId, string section, string key);

        Task<NameValueCollection> GetTenantConfigurationAsNameValueCollectionAsync(string tenantId, string section, string key);

        Task<T> GetTenantConfigurationAsync<T>(string tenantId, string section, string key);
        /// <summary>
        ///  applicationName will override applicatioName passed in constructor
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="applicationName"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<string> GetTenantConfigurationAsStringAsync(string tenantId, string applicationName, string section, string key);

        /// <summary>
        /// applicationName will override applicatioName passed in constructor
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="applicationName"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<NameValueCollection> GetTenantConfigurationAsNameValueCollectionAsync(string tenantId, string applicationName, string section, string key);

        /// <summary>
        /// applicationName will override applicatioName passed in constructor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tenantId"></param>
        /// <param name="applicationName"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<T> GetTenantConfigurationAsync<T>(string tenantId, string applicationName, string section, string key);
        #endregion

        #region SyncMethods


        string GetFeatureFlagSettingsAsString(string tenantId, string key);
        NameValueCollection GetFeatureFlagSettingsAsNameValueCollection(string tenantId, string key);
        T GetFeatureFlagSettingsAsObject<T>(string tenantId, string key);

        string GetFeatureFlagSettingsAsString(string tenantId, string applicationName, string key);
        NameValueCollection GetFeatureFlagSettingsAsNameValueCollection(string tenantId, string applicationName, string key);
        T GetFeatureFlagSettingsAsObject<T>(string tenantId, string applicationName, string key);

        string GetGlobalConfigurationAsString(string section, string key);

        NameValueCollection GetGlobalConfigurationAsNameValueCollection(string section, string key);

        T GetGlobalConfiguration<T>(string section, string key);

        /// <summary>
        /// applicationName will override applicatioName passed in constructor
        /// </summary>
        /// <param name="applicationName"> </param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetGlobalConfigurationAsString(string applicationName, string section, string key);

        /// <summary>
        /// applicationName will override applicatioName passed in constructor
        /// </summary>
        /// <param name="applicationName"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        NameValueCollection GetGlobalConfigurationAsNameValueCollection(string applicationName, string section, string key);

        /// <summary>
        /// applicationName will override applicatioName passed in constructor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="applicationName"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        T GetGlobalConfiguration<T>(string applicationName, string section, string key);

        string GetTenantConfigurationAsString(string tenantId, string section, string key);

        NameValueCollection GetTenantConfigurationAsNameValueCollection(string tenantId, string section, string key);

        T GetTenantConfiguration<T>(string tenantId, string section, string key);
        /// <summary>
        ///  applicationName will override applicatioName passed in constructor
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="applicationName"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetTenantConfigurationAsString(string tenantId, string applicationName, string section, string key);

        /// <summary>
        /// applicationName will override applicatioName passed in constructor
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="applicationName"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        NameValueCollection GetTenantConfigurationAsNameValueCollection(string tenantId, string applicationName, string section, string key);

        /// <summary>
        /// applicationName will override applicatioName passed in constructor
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tenantId"></param>
        /// <param name="applicationName"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        T GetTenantConfiguration<T>(string tenantId, string applicationName, string section, string key);

        #endregion

        #region Health Check
        Task<ConfigurationProviderConnectionStatus> HealthCheckAsync();
        #endregion
    }
}