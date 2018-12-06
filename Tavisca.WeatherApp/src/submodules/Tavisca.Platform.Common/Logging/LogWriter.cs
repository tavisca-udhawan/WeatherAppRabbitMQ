using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Configurations;

namespace Tavisca.Platform.Common.Logging
{
    public class LogWriter : IApplicationLogWriter
    {
        public LogWriter(ILogFormatter formatter, ILogSink sink, IEnumerable<ILogFilter> filters = null, IConfigurationProvider configurationProvider = null)
        {
            Formatter = formatter;
            Sink = sink;
            ConfigurationProvider = configurationProvider;
            if (filters != null)
                Filters.AddRange(filters);
        }

        public ILogFormatter Formatter { get; }
        public ILogSink Sink { get; }
        public IConfigurationProvider ConfigurationProvider { get; }
        public List<ILogFilter> Filters { get; } = new List<ILogFilter>();

        public async Task WriteAsync(ILog log)
        {
            /*
             * DESIGN:
             * For performance, all log writing is to be done asynchronously in a separate thread pool.
             * Also, depending on whether the logs need sanitization (i.e., removal of PII / card data), different pools are being
             * used. This is because sanization is a slow process which needs to parse the content. Logs which do not need sanitization
             * can be logged directly as part of the fast path.
             * 
             * NAMING CONVENTION:
             * Some internal naming conventions:
             *      byte[] attribute names will start with stream_
             *      geopoint attribute names will start with geo_
             *      Dictionary attribute names will start with json_
             *  This naming convention will help log aggregates decide on how to handle these special fields.
             *  
             * FORMATTING
             * The log object would be in JSON format.
             *      - byte[] fields would be compressed and subsequently base64 encoded so that they can be put inside the json payload.
             *      - geopoint fields would be json of the format { "lat" : 123.45, "lon" : 456.78 }
             *      - dictionary fields would be converted to their corresponding json.
            */
            
            if (await IsLoggingDisabled() == false)
                await ProcessAndWriteLogAsync(log);
        }

        private async Task<bool> IsLoggingDisabled()
        {
            var configProviderCopy = ConfigurationProvider;
            if(configProviderCopy != null)
                return await configProviderCopy.GetGlobalConfigurationAsync<bool>(KeyStore.ConfigurationKeys.Logging, KeyStore.ConfigurationKeys.IsLoggingDisabled);
            return false;
        }

        private Task ProcessAndWriteLogAsync(ILog log)
        {
            AsyncTasks.Run(() =>
               {
                   var sanitized = ApplyFilters(log);
                   WriteLogAsync(sanitized).GetAwaiter().GetResult();
               }, "logging");
            return Task.CompletedTask;
        }

        private ILog ApplyFilters(ILog log)
        {
            var logFilters = log.Filters;

            // Apply default filtering
            foreach (var filter in Filters)
                log = filter.Apply(log);
            // Apply log specific filters
            foreach (var filter in logFilters)
                log = filter.Apply(log);
            return log;
        }

        private async Task WriteLogAsync(ILog log)
        {
            var data = Formatter.Format(log);
            await Sink.WriteAsync(log, data);
        }
    }

    public interface ILogFormatter
    {
        byte[] Format(ILog log);
    }

    public interface ILogSink
    {
        Task WriteAsync(ILog log, byte[] logData);
    }
}
