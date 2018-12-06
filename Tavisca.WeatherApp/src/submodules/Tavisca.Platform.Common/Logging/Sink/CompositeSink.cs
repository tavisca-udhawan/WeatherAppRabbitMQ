using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Logging
{
    public class CompositeSink : ILogSink
    {
        private readonly ILogFormatter _formatter;
        private readonly ILogSink[] _logSinks;

        public CompositeSink(ILogFormatter formatter, IEnumerable<ILogSink> logSinks)
        {
            _formatter = formatter;
            _logSinks = logSinks != null ? logSinks.ToArray() : new ILogSink[0];
        }

        public async Task WriteAsync(ILog log, byte[] logData)
        {
            List<Exception> exceptions = new List<Exception>();
            ILogSink activeLogSink = null;
            foreach (var logSink in _logSinks)
            {
                try
                {
                    activeLogSink = logSink;
                    await activeLogSink.WriteAsync(log, logData);
                    break;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0 && activeLogSink != null)
                await WriteExceptionsAsync(activeLogSink, log, exceptions);
        }

        private async Task WriteExceptionsAsync(ILogSink logSink, ILog log, List<Exception> exceptions)
        {
            if (exceptions.Count > 0)
            {
                try
                {
                    foreach (var exception in exceptions)
                    {
                        var exceptionLog = GetErrorEntry(exception, log);
                        await logSink.WriteAsync(exceptionLog, Format(exceptionLog));
                    }
                }
                catch (Exception ex)
                {
                    throw new AggregateException(ex.Message, exceptions);
                }
            }
        }

        private ExceptionLog GetErrorEntry(Exception exception, ILog log)
        {
            var exceptionLog = new ExceptionLog(exception);

            var baseLog = log as LogBase;
            if (baseLog == null)
                return exceptionLog;

            exceptionLog.AppDomain = baseLog.AppDomain;
            exceptionLog.MachineName = baseLog.MachineName;
            exceptionLog.ApplicationName = baseLog.ApplicationName;
            exceptionLog.TenantId = baseLog.TenantId;
            exceptionLog.CorrelationId = baseLog.CorrelationId;
            exceptionLog.StackId = baseLog.StackId;

            return exceptionLog;
        }

        private byte[] Format(ILog log)
        {
            var data = _formatter.Format(log);
            return data;
        }
    }
}
