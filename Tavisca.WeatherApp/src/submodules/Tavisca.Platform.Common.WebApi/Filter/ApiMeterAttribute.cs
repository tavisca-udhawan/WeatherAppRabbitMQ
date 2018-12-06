using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Tavisca.Platform.Common.Metrics;
using Tavisca.Platform.Common.Models;

namespace Tavisca.Platform.Common.WebApi
{
    public class ApiMeterAttribute : ActionFilterAttribute
    {
        private Stopwatch _stopwatch;
        private readonly string _meterKeyPrefix;
        private IMeter _meter;

        public ApiMeterAttribute(string meterKeyPrefix)
        {
            if (string.IsNullOrEmpty(meterKeyPrefix))
                throw new ArgumentNullException(nameof(meterKeyPrefix), "Meter key prefix cannot be null or empty");

            _meterKeyPrefix = meterKeyPrefix;
        }

        public override async Task OnActionExecutingAsync(HttpActionContext actionContext,
            CancellationToken cancellationToken)
        {
            _meter = Metering.GetGlobalMeter();
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            AddCountMetric("count");       //meters the count of api call

            await base.OnActionExecutingAsync(actionContext, cancellationToken);
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext,
            CancellationToken cancellationToken)
        {
            if (actionExecutedContext.Exception != null && IsCriticalFault(actionExecutedContext.Exception))
                AddCountMetric("failure");      //meters the failure count of the api call

            _stopwatch.Stop();
            AddTimerMetric("latency", _stopwatch.ElapsedMilliseconds);  //meters the latency of the api call

            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }

        private void AddCountMetric(string metricName)
        {
            var key = string.IsNullOrEmpty(metricName) ? _meterKeyPrefix : $"{_meterKeyPrefix}.{metricName}";
            _meter.Meter(key);
        }

        private void AddTimerMetric(string metricName, long milliseconds)
        {
            var key = string.IsNullOrEmpty(metricName) ? _meterKeyPrefix : $"{_meterKeyPrefix}.{metricName}";
            _meter.Timer(key, TimeSpan.FromMilliseconds(milliseconds));
        }

        private static bool IsCriticalFault(Exception exception)
        {
            var appEx = exception as BaseApplicationException;
            var isSystemException = appEx == null;
            var isCriticalAppException = appEx != null && (int) appEx.HttpStatusCode >= 500;
            return (isSystemException == true || isCriticalAppException == true);
        }
    }
}