using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Tavisca.Platform.Common;
using Tavisca.Platform.Common.Context;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Metrics;
using Tavisca.Platform.Common.Plugins.Json;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    public class ServiceCallerLogger
    {
        public string Api { get; set; }
        public string Verb { get; set; }
        public string Message { get; set; }
        public string SessionId { get; set; }
        public string ApiMeterPrefix { get; set; }
        public string HttpMethod { get; set; }
        public List<ILogFilter> LogFilters { get; } = new List<ILogFilter>();

        public ServiceCallerLogger(string api, string verb, string httpMethod, string sessionId, string apiMeterPrefix = null, string message = null, List<ILogFilter> logFilters = null)
        {
            Api = api;
            Verb = verb;
            HttpMethod = httpMethod;
            Message = message;
            SessionId = sessionId;
            ApiMeterPrefix = apiMeterPrefix;
            if (logFilters != null)
                LogFilters.AddRange(logFilters);
        }

        public async Task Log(object sender, LogEventArgs logEventArgs)
        {
            var context = CallContext.Current;
            var log = new ApiLog
            {
                ApplicationName = context.ApplicationName,
                Api = Api,
                Verb = Verb,
                TenantId = context.TenantId,
                CorrelationId = context.CorrelationId,
                StackId = context.StackId,
                ApplicationTransactionId = context.TransactionId,
                TransactionId = logEventArgs.ResponseHeaders != null ? logEventArgs.ResponseHeaders[HeaderNames.TransactionId] : null,
                ClientIp = context.IpAddress,
                Message = Message,
                TimeTakenInMs = logEventArgs.TimeTakenInSeconds * 1000,
                LogTime = logEventArgs.Timestamp,
                Url = logEventArgs.Url,
                IsSuccessful = (int)logEventArgs.HttpStatusCode / 100 == 2
            };

            log.SetValue("session_id", SessionId);
            log.SetValue("http_method", HttpMethod);
            log.SetValue("http_status_code", (int)logEventArgs.HttpStatusCode);

            if (string.IsNullOrEmpty(logEventArgs.RequestString) == false)
                log.Request = new Payload(logEventArgs.RequestString);
            else if (logEventArgs.Request != null)
                log.Request = new Payload(ByteHelper.ToByteArrayUsingJsonSerialization(logEventArgs.Request));

            if (string.IsNullOrEmpty(logEventArgs.ResponseString) == false)
                log.Response = new Payload(logEventArgs.ResponseString);
            else if (logEventArgs.Response != null)
                log.Response = new Payload(ByteHelper.ToByteArrayUsingJsonSerialization(logEventArgs.Response));

            if (logEventArgs.RequestHeaders?.AllKeys != null)
                foreach (var key in logEventArgs.RequestHeaders?.AllKeys)
                    log.RequestHeaders[key] = logEventArgs.RequestHeaders[key];

            if (logEventArgs.ContentHeaders?.AllKeys != null)
                foreach (var key in logEventArgs.ContentHeaders?.AllKeys)
                    log.ResponseHeaders[key] = logEventArgs.ContentHeaders[key];

            if (logEventArgs.ResponseHeaders?.AllKeys != null)
                foreach (var key in logEventArgs.ResponseHeaders?.AllKeys)
                    log.ResponseHeaders[key] = logEventArgs.ResponseHeaders[key];

            log.Filters.AddRange(LogFilters);

            await Platform.Common.Logging.Logger.WriteLogAsync(log);

            HandleApiMetering(logEventArgs);
        }

        public Task LogOnFailure(object sender, LogEventArgs logEventArgs)
        {
            if (logEventArgs != null && logEventArgs.HttpStatusCode != HttpStatusCode.OK)
            {
                return Log(sender, logEventArgs);
            }
            return Task.CompletedTask;
        }

        private void HandleApiMetering(LogEventArgs logEventArgs)
        {
            if (string.IsNullOrWhiteSpace(ApiMeterPrefix))
                return;

            IMeter meter = Metering.GetGlobalMeter();
            if (meter == null)
                return;

            int httpStatusCode = (int)logEventArgs.HttpStatusCode;
            if (httpStatusCode >= 500)
            {
                meter.Meter($"{ApiMeterPrefix}.failure");
            }
            meter.Meter($"{ApiMeterPrefix}.count");
            meter.Timer($"{ApiMeterPrefix}.latency", TimeSpan.FromSeconds(logEventArgs.TimeTakenInSeconds));
        }
    }
}
