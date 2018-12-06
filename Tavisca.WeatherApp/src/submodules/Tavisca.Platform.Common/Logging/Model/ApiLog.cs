using System;
using System.Collections.Generic;
using System.Net;

namespace Tavisca.Platform.Common.Logging
{
    [Serializable]
    public class ApiLog : LogBase
    {
        public string Url { get; set; }

        public string Api { get; set; }

        public string Verb { get; set; }

        public IPAddress ClientIp { get; set; }

        public IDictionary<string, string> RequestHeaders { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IDictionary<string, string> ResponseHeaders { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public Payload Request { get; set; }

        public Payload Response { get; set; }

        public bool IsSuccessful { get; set; }

        public double TimeTakenInMs { get; set; }

        public string TransactionId { get; set; }

        public override string Type { get; } = "api";

        protected override List<KeyValuePair<string, object>> GetLogFields()
        {
            return new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object> ("url", Url),
                new KeyValuePair<string, object> ("api", Api),
                new KeyValuePair<string, object> ("verb", Verb),
                new KeyValuePair<string, object> ("status", IsSuccessful ? "success" : "failure"),
                new KeyValuePair<string, object> ("time_taken_ms", Math.Round(TimeTakenInMs,3)),
                new KeyValuePair<string, object> ("txid", TransactionId),
                new KeyValuePair<string, object> ("request", Request),
                new KeyValuePair<string, object> ("response", Response),
                new KeyValuePair<string, object> ("client_ip", ClientIp),
                new KeyValuePair<string, object> ("rq_headers", new Map(RequestHeaders, MapFormat.Json)),
                new KeyValuePair<string, object> ("rs_headers", new Map(ResponseHeaders, MapFormat.Json))
            };
        }
    }
    
}
