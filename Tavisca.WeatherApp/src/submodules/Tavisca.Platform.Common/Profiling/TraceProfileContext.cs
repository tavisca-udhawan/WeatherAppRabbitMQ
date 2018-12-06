using System;
using System.Diagnostics;
using Tavisca.Platform.Common.Context;
using Tavisca.Platform.Common.Logging;

namespace Tavisca.Platform.Common.Profiling
{
    public class TraceProfileContext : ProfileContext, IDisposable
    {
        Stopwatch watch;
        string TraceMessage, TraceCategory;

        public TraceProfileContext(string category, string message) : base(message)
        {
            watch = Stopwatch.StartNew();
            TraceMessage = message;
            TraceCategory = category;
        }
        public void Dispose()
        {
            watch.Stop();

            base.Dispose();

            if (CallContext.Current?.IsProfilingEnabled == true)
            {
                var log = new TraceLog
                {
                    Message = TraceMessage,
                    Category = TraceCategory,
                    ApplicationName = CallContext.Current?.ApplicationName,
                    CorrelationId = CallContext.Current?.CorrelationId,
                    StackId = CallContext.Current?.StackId,
                    TenantId = CallContext.Current?.TenantId,
                };
                log.SetValue("time_taken_ms", watch.ElapsedMilliseconds);

                Platform.Common.Logging.Logger.WriteLogAsync(log);
            }
        }
    }
}
