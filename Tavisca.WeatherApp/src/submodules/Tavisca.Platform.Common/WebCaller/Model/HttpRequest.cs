using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common
{
    public class HttpRequest
    {
        public long? MaxResponseBufferSize { get; set; }
        public TimeSpan? TimeOut { get; set; }
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public Uri Uri { get; private set; }
        public string ProtocolVersion { get; set; }
        public IContent Content { get; internal set; }
        public byte[] Payload { get; internal set; }
        public string ContentType { get; set; }
        public IFaultPolicy FaultPolicy { get; set; }
        public List<HttpFilter> HttpFilters { get; } = new List<HttpFilter>();
        public HttpSettings Settings { get; set; }
        public string Method { get; set; }

        public bool? IsProxyDisabled { get; set; }
        public Dictionary<string, object> LogData { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        //Ref: https://en.wikipedia.org/wiki/Hypertext_Transfer_Protocol
        private static readonly HashSet<string> MethodsWithoutBody = new HashSet<string> { "GET", "DELETE", "TRACE", "HEAD" };
        internal bool SendRequestBody => MethodsWithoutBody.Contains(Method) == false || Payload?.Length == 0;


        public HttpRequest(Uri uri, HttpSettings settings = null)
        {
            Uri = uri;
            CompressionType = CompressionType.None;
            Settings = HttpSettings.Resolve(settings, HttpSettings.Default);
            ApplySettings(Settings);
        }

        public bool IsCompressionEnabled
        {
            get
            {
                return CompressionType != CompressionType.None;
            }
        }

        public CompressionType CompressionType { get; set; }

        public async Task<HttpResponse> SendAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            HttpResponse httpResponse = null;
            // Add connector filter at the end of the pipeline
            var finalFilters = SetupHttpFilters();
            //get payload from Content field (this can be null for http methods like GET, DELETE, etc.)
            if (Content != null)
                this.Payload = await Content.GetPayloadAsync(Settings);
            httpResponse = await finalFilters[0].ApplyAsync(this, cancellationToken);

            return httpResponse;
        }

        [Obsolete("Please use HttpRequest Extension method : WithBody<T>")]
        public Task SetBodyAsync<T>(T request)
        {
            Content = new ObjectContent<T>(request);
            return Task.CompletedTask;
        }

        private List<HttpFilter> SetupHttpFilters()
        {
            List<HttpFilter> result = new List<HttpFilter>();
            var connector = Settings.BuildConnector();
            var connectorHttpFilter = new HttpConnectorFilter(connector);
            if (HttpFilters.Count > 0)
            {
                for (int i = 0; i < HttpFilters.Count - 1; i++)
                {
                    HttpFilters[i].SetInnerFilter(HttpFilters[i + 1]);
                }
                HttpFilters[HttpFilters.Count - 1].SetInnerFilter(connectorHttpFilter);
                result.AddRange(HttpFilters);
                return result;
            }
            return new List<HttpFilter>()
            {
                connectorHttpFilter
            };
        }

        private void ApplySettings(HttpSettings settings)
        {
            if (settings == null) return;
            MaxResponseBufferSize = MaxResponseBufferSize ?? settings.MaxResponseBufferSize;
            TimeOut = TimeOut ?? settings.TimeOut;
            ProtocolVersion = string.IsNullOrEmpty(ProtocolVersion) ? settings.ProtocolVersion : ProtocolVersion;
            ContentType = string.IsNullOrEmpty(ContentType) ? settings.ContentType : ContentType;
            foreach (var header in settings.Headers)
            {
                if (Headers.ContainsKey(header.Key) == false)
                    Headers[header.Key] = header.Value;
            }
            IsProxyDisabled = settings.IsProxyDisabled;
        }
    }
}
