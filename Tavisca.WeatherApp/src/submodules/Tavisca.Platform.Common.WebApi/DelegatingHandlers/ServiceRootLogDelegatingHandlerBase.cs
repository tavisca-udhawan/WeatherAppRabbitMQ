using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Tavisca.Platform.Common.Context;
using Tavisca.Platform.Common.Logging;

namespace Tavisca.Platform.Common.WebApi
{
    public abstract class ServiceRootLogDelegatingHandlerBase : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var startTimestamp = DateTime.UtcNow;
            HttpResponseMessage response = null;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
                return response;
            }
            finally
            {
                if (ShouldLog(request, response))
                    await Log(request, response, stopWatch, startTimestamp);
            }
        }

        private async Task Log(HttpRequestMessage request, HttpResponseMessage response, Stopwatch stopWatch, DateTime startTimestamp)
        {
            var apiLogEntry = await GetLog(request, response);
            apiLogEntry.LogTime = startTimestamp;

            if (stopWatch.IsRunning)
                stopWatch.Stop();

            apiLogEntry.TimeTakenInMs = stopWatch.ElapsedMilliseconds;
            await Logger.WriteLogAsync(apiLogEntry);
        }

        protected abstract Task<ApiLog> GetLog(HttpRequestMessage request, HttpResponseMessage response);

        protected abstract bool ShouldLog(HttpRequestMessage request, HttpResponseMessage response);
    }
}