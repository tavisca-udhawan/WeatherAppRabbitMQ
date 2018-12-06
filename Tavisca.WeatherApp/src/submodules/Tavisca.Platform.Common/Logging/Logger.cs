using System.Threading.Tasks;
using Tavisca.Platform.Common.Configurations;

namespace Tavisca.Platform.Common.Logging
{
    public static class Logger
    {
        private static ILogWriterFactory _factory;
        private static IConfigurationProvider _configurationProvider;

        public static void Initialize(ILogWriterFactory factory)
        {
            _factory = factory;
        }
        public static Task WriteLogAsync(ILog log)
        {
            try
            {
                _factory?
                    .CreateWriter()?
                    .WriteAsync(log);
            }
            catch
            {
                // ignored
            }
            return Task.CompletedTask;
        }
    }
}
