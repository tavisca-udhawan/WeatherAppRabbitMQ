using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.Platform.Common.Logging;
using Tavisca.WeatherApp.Models.Models;
using LogWriter = Tavisca.Platform.Common.Logging;
namespace Tavisca.WeatherApp.Models.Logging
{
    public static class Loggers
    {
        private static readonly ILogWriterFactory _logWriterFactory = new LogWriterFactory();

        public static void AddApiLog(string apiName, string verb, byte[] request, byte[] response, string url, TimeSpan? timeTaken = null)
        {
            var apiLog = new LogWriter.ApiLog();
            if (timeTaken.HasValue)
                apiLog.LogTime = DateTime.UtcNow.Subtract(timeTaken.Value);
            else
                apiLog.LogTime = DateTime.UtcNow;

            apiLog.ApplicationName = KeyStore.ApplicationName;
            apiLog.Request = new Payload(request);
            apiLog.Response = new Payload(response);
            apiLog.Url = url;
            apiLog.Api = apiName;
            apiLog.Verb = verb;
            apiLog.SetValue("owner", "udhawan");

            LogWriter.Logger.Initialize(_logWriterFactory);
            LogWriter.Logger.WriteLogAsync(apiLog);
        }

        public static void AddTrace(string message, TimeSpan? timeTaken = null)
        {
            var traceLog = new LogWriter.TraceLog();
            if (timeTaken.HasValue)
                traceLog.LogTime = DateTime.UtcNow.Subtract(timeTaken.Value);
            else
                traceLog.LogTime = DateTime.UtcNow;

            traceLog.ApplicationName = KeyStore.ApplicationName;
            traceLog.Message = message;
            traceLog.SetValue("owner", "udhawan");

            LogWriter.Logger.Initialize(_logWriterFactory);
            LogWriter.Logger.WriteLogAsync(traceLog);
        }
    }
}
