using System;
using System.Collections.Generic;
using Tavisca.Platform.Common.Context;

namespace Tavisca.Platform.Common.Logging
{
    [Serializable]
    public class TraceLog : LogBase
    {
        public string Category { get; set; }

        public override string Type { get; } = "trace";

        protected override List<KeyValuePair<string, object>> GetLogFields()
        {
            return new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object> ("category", Category)
            };
        }

        public static TraceLog GenerateTraceLog(string message)
        {
            TraceLog trace = new TraceLog();
            CallContext context = CallContext.Current;

            if (context != null)
            {
                trace.ApplicationName = context.ApplicationName;
                trace.TenantId = context.TenantId;
                trace.CorrelationId = context.CorrelationId;
                trace.StackId = context.StackId;
                trace.ApplicationTransactionId = context.TransactionId;
                trace.Message = message;

                if (!string.IsNullOrWhiteSpace(context.UserToken))
                    trace.SetValue(HeaderNames.UserToken, context.UserToken);
            }

            return trace;
        }
    }
}
