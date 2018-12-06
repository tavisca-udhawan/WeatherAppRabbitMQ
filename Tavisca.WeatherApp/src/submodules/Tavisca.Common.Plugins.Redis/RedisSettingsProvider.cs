using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.LockManagement;
using Tavisca.Platform.Common.Profiling;

namespace Tavisca.Common.Plugins.Redis
{
    public class RedisSettingsProvider : ILockSettingsProvider, ICacheSettingsProvider
    {
        private readonly IConfigurationProvider _configurationProvider;
        public RedisSettingsProvider(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }

        public async Task<RedisSettings> GetCacheSettingsAsync()
        {
            using (new ProfileContext("fetching redis lockProvider settings"))
            {
                return await _configurationProvider.GetGlobalConfigurationAsync<RedisSettings>(
                Keystore.RedisKeys.CacheSettings, Keystore.RedisKeys.RedisSettings);
            }
        }

        public async Task<ILockSettings> GetSettings()
        {
            using (new ProfileContext("fetching redis lockProvider settings"))
            {
                return await _configurationProvider.GetGlobalConfigurationAsync<RedisLockSettings>(
                Keystore.RedisKeys.RedisSettings, Keystore.RedisKeys.LockSettings);
            }
        }
    }
}
