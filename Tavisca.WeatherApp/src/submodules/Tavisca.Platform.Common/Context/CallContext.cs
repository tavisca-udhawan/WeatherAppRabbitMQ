using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;

namespace Tavisca.Platform.Common.Context
{
    [Serializable]
    public abstract class CallContext : AmbientContextBase
    {
        public string ApplicationName { get; protected internal set; }

        public bool IsProfilingEnabled { get; protected internal set; }

        public CultureInfo Culture { get; protected internal set; }

        public string CorrelationId { get; protected internal set; }

        public IPAddress IpAddress { get; protected internal set; }

        public string TenantId { get; protected internal set; }

        public string UserToken { get; protected internal set; }

        public string StackId { get; protected internal set; }

        public string TransactionId { get; protected internal set; }

        public NameValueCollection Headers { get; } = new NameValueCollection();

        public new static CallContext Current => (CallContext) AmbientContextBase.Current;
    }

}
