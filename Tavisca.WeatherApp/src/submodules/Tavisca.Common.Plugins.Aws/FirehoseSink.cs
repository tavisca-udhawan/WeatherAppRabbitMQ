using System.Collections.Specialized;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.Logging;

namespace Tavisca.Common.Plugins.Aws
{
    public class FirehoseSink : ILogSink
    {
        private readonly IConfigurationProvider _config = null;
        private readonly FirehoseClientFactory _clientFactory = new FirehoseClientFactory();
        public FirehoseSink(IConfigurationProvider config)
        {
            _config = config;
        }

        private async Task<FirehoseSettings> GetSettings()
        {
            var nvc = await _config.GetGlobalConfigurationAsNameValueCollectionAsync("logging", "firehose");
            
            return FirehoseSettings.Load(nvc ?? new NameValueCollection());
        }

        public async Task WriteAsync(ILog log, byte[] logData)
        {
            var settings = await GetSettings();

            var client = await _clientFactory.CreateAsync(settings);

            await client.WriteAsync(logData, settings.Stream);
        }
    }

}
