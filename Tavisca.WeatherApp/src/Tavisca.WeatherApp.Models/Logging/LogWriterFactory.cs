using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.Common.Plugins.Redis;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Plugins.Json;

namespace Tavisca.WeatherApp.Models.Logging
{
    public class LogWriterFactory : ILogWriterFactory
    {
        private ILogSink logSink;
        public IApplicationLogWriter CreateWriter()
        {
            logSink = new RedisSink("P-QA-LoggingQueue", new List<string> { "private-redislogs.qa.oski.io:6379" });
            return new LogWriter(JsonLogFormatter.Instance, logSink);
        }
    }
    public enum LoggingSink
    {
        Redis,
        Firehose,
        Composite
    }
}
