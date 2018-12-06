using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Configuration
{
    public interface IConfigurationStore
    {
        Task<string> GetAsync(string scope, string application, string section, string key);

        string Get(string scope, string application, string section, string key);

        Task<Dictionary<string, string>> GetAllAsync();

        Task<ConfigurationStoreConnectionStatus> HealthCheckAsync();
    }

    public enum ConfigurationStoreConnectionStatus
    {
        Connected,
        Disconnected
    }
}
