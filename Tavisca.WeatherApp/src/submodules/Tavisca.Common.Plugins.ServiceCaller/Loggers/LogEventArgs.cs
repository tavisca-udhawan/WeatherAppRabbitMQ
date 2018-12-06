using System;
using System.Collections.Specialized;
using System.Net;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    public class LogEventArgs : EventArgs
    {
        public object Request { get; internal set; }
        public string RequestString { get; internal set; }
        public object Response { get; internal set; }
        public string ResponseString { get; internal set; }
        public double TimeTakenInSeconds { get; internal set; }
        public HttpStatusCode HttpStatusCode { get; internal set; }
        public NameValueCollection ResponseHeaders { get; internal set; }
        public NameValueCollection ContentHeaders { get; internal set; }
        public NameValueCollection RequestHeaders { get; internal set; }
        public string Url { get; internal set; }
        public DateTime Timestamp { get; set; }
    }
}
