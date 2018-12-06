using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Configurations;

namespace Tavisca.Common.Plugins.Redis
{
    public class RedisSink : ILogSink
    {
        private readonly IConfigurationProvider _config;
        private Func<Task<RedisSettings>> _getSettings = null;
        private readonly RedisClientFactory _clientFactory = new RedisClientFactory();

        public RedisSink(string queueName, IEnumerable<string> hosts)
        {
            var settings = new RedisSettings() { QueueName = queueName };
            settings.Hosts.AddRange(hosts);
            _getSettings = () => Task.FromResult(settings);
        }

        public RedisSink(IConfigurationProvider config)
        {
            _config = config;
            _getSettings = () => GetSettings();
        }
        public async Task WriteAsync(ILog log, byte[] logData)
        {
            await WriteSafely(logData);
        }

        private async Task<RedisSettings> GetSettings()
        {
            var settings = await _config.GetGlobalConfigurationAsync<RedisSettings>("logging", "redis_settings");
            if (settings == null)
                throw new ArgumentNullException($"Missing configuration for logging.redis_settings");
            return settings;
        }

        private async Task WriteSafely(byte[] logData)
        {
            var settings = await _getSettings();

            try
            {
                using (var client = _clientFactory.Create(settings))
                {
                    client.PushItemToList(settings.QueueName, ByteHelper.CompressAndEncode(logData));
                }
            }
            catch
            {
                //throw exception so it can be logged in eventviewer
                throw;
            }
        }
    }
}