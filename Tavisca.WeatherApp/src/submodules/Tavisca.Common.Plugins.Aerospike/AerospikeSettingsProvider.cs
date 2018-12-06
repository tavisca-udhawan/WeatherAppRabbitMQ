using System.Threading.Tasks;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.Profiling;

namespace Tavisca.Common.Plugins.Aerospike
{
    public class AerospikeSettingsProvider : ILockProviderSettings, IStateProviderSettings, ICounterSettings
    {
        private readonly IConfigurationProvider _configurationProvider;
        public AerospikeSettingsProvider(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }

        Task<AerospikeSettings> ICounterSettings.GetSettings()
        {
            using (new ProfileContext("fetching aerospike(counter) settings"))
                return _configurationProvider.GetGlobalConfigurationAsync<AerospikeSettings>(
                Keystore.AerospikeKeys.SettingsSection, Keystore.AerospikeKeys.CounterSettings);
        }

        Task<AerospikeSettings> IStateProviderSettings.GetSettings()
        {
            using (new ProfileContext("fetching aerospike(stateProvider) settings"))
                return _configurationProvider.GetGlobalConfigurationAsync<AerospikeSettings>(
                Keystore.AerospikeKeys.SettingsSection, Keystore.AerospikeKeys.StateSettings);
        }

        Task<AerospikeSettings> ILockProviderSettings.GetSettings()
        {
            using (new ProfileContext("fetching aerospike(lockProvider) settings"))
                return _configurationProvider.GetGlobalConfigurationAsync<AerospikeSettings>(
                Keystore.AerospikeKeys.SettingsSection, Keystore.AerospikeKeys.LockSettings);
        }

    }
}
