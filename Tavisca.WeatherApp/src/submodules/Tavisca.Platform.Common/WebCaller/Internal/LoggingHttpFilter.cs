using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Context;
using Tavisca.Platform.Common.Logging;

namespace Tavisca.Platform.Common
{
    public class LoggingHttpFilter : HttpFilter, ILoggingHttpFilter
    {
        public List<ILogFilter> Filters { get; } = new List<ILogFilter>();

        public LoggingHttpFilter(List<ILogFilter> filters = null)
        {
            if (filters != null)
                Filters.AddRange(filters);
        }
        public async override Task<HttpResponse> ApplyAsync(HttpRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            HttpResponse httpResponse = null;
            Exception fault = null;
            var watch = Stopwatch.StartNew();
            try
            {
                httpResponse = await base.ApplyAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                fault = ex;
                throw;
            }
            finally
            {
                watch.Stop();

                if (fault == null)
                {
                    var faultPolicy = request.FaultPolicy ?? DefaultFaultPolicy.Instance;
                    var isSuccessful = await faultPolicy.IsFaultedAsync(request, httpResponse);
                    await LogAsync(request, httpResponse, isSuccessful, watch.Elapsed, CancellationToken.None);
                }
                else
                    await LogFaultAsync(request, fault, watch.Elapsed, CancellationToken.None);
            }
            return httpResponse;
        }

        private async Task LogAsync(HttpRequest request, HttpResponse response, bool isSuccessful, TimeSpan timeTaken, CancellationToken cancellationToken = default(CancellationToken))
        {
            await ProcessApiLogAsync(request, response, isSuccessful, timeTaken, cancellationToken);
        }

        private async Task LogFaultAsync(HttpRequest request, Exception ex, TimeSpan timeTaken, CancellationToken cancellationToken = default(CancellationToken))
        {
            await ProcessApiLogAsync(request, null, false, timeTaken, cancellationToken);
            await ProcessExceptionLogAsync(request, ex, timeTaken, cancellationToken);
        }

        private async Task ProcessExceptionLogAsync(HttpRequest request, Exception ex, TimeSpan timeTaken, CancellationToken cancellationToken)
        {
            ILog apiExceptionLog = CreateExceptionLog(request, ex);
            await Logger.WriteLogAsync(apiExceptionLog);
        }

        private async Task ProcessApiLogAsync(HttpRequest request, HttpResponse response, bool isSuccessful, TimeSpan timeTaken, CancellationToken cancellationToken = default(CancellationToken))
        {
            ILog apiLog = CreateApiLog(request, response, isSuccessful, timeTaken);
            await Logger.WriteLogAsync(apiLog);
        }

        protected virtual ILog CreateExceptionLog(HttpRequest request, Exception ex)
        {
            var apiExceptionLog = new ExceptionLog(ex);
            apiExceptionLog.SetValue("type", ex.GetType().ToString());

            SetLogDataToLog(request.LogData, apiExceptionLog);
            return apiExceptionLog;
        }

        protected virtual ILog CreateApiLog(HttpRequest request, HttpResponse response, bool isSuccessful, TimeSpan timeTaken)
        {
            var requestLogData = request.LogData.ToList();

            var apiLog = new ApiLog()
            {
                ApplicationName = CallContext.Current?.ApplicationName,
                CorrelationId = CallContext.Current?.CorrelationId,
                TenantId = CallContext.Current?.TenantId,
                StackId = CallContext.Current?.StackId,
                ApplicationTransactionId = CallContext.Current?.TransactionId,
                ClientIp = CallContext.Current?.IpAddress,
                TimeTakenInMs = timeTaken.TotalMilliseconds,
                Url = request.Uri.AbsoluteUri,
                IsSuccessful = isSuccessful
            };
            string transactionId;
            if (response != null && response.Headers.TryGetValue(HeaderNames.TransactionId, out transactionId))
                apiLog.TransactionId = transactionId;

            if (response != null)
                apiLog.SetValue("http_status_code", (int)response.Status);

            apiLog.SetValue("request_method", request.Method);

            if (request?.Payload != null)
                apiLog.Request = new Payload(request.Payload);

            if (response?.Payload != null)
                apiLog.Response = new Payload(response.Payload);

            if (request.Headers?.Keys != null)
                foreach (var key in request.Headers?.Keys)
                    apiLog.RequestHeaders[key] = request.Headers[key];

            if (response?.Headers?.Keys != null)
                foreach (var key in response.Headers?.Keys)
                    apiLog.ResponseHeaders[key] = response.Headers[key];

            if (Filters != null)
                apiLog.Filters.AddRange(Filters);

            // Always log the white listed headers as they are required for reporting purpose
            var whiteListedHeaders = CallContext.Current?.Headers;
            if (whiteListedHeaders != null)
            {
                foreach (var key in whiteListedHeaders.AllKeys.ToList())
                    apiLog.RequestHeaders[key] = whiteListedHeaders[key];
            }

            SetLogDataToLog(request?.LogData, apiLog);
            SetLogDataToLog(response?.LogData, apiLog);
            return apiLog;
        }

        private void SetLogDataToLog(Dictionary<string, object> logData, LogBase log)
        {
            if (logData != null)
            {
                logData.Keys.ToList().ForEach(key =>
                {
                    if (log.TrySetValue(key, logData[key]))
                    {
                        // TODO: add handling for values that can't be added
                    }
                });
            }
        }
    }
}
