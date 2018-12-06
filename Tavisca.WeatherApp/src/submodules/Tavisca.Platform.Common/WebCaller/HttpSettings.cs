using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using Tavisca.Platform.Common.MemoryStreamPool;
using Tavisca.Platform.Common.Serialization;

namespace Tavisca.Platform.Common
{
    public class HttpSettings
    {
        internal static readonly HttpSettings Default = new HttpSettings();

        public HttpSettings()
        {
            ProtocolVersion = "1.1";
            TimeOut = new TimeSpan(0, 0, 0, 60);
            MaxResponseBufferSize = 65556;
            ContentType = "application/json";
            MemoryPool = new DefaultMemoryStreamPool();
        }

        public long? MaxResponseBufferSize { get; set; }
        public TimeSpan? TimeOut { get; set; }
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public string ProtocolVersion { get; set; }
        public string ContentType { get; set; }
        public IMemoryStreamPool MemoryPool { get; set; }
        public bool? IsProxyDisabled { get; set; }
        public ISerializer Serializer { get; set; }

        [Obsolete("Use the WithConnector(Func<IHttpConnector> connectorFactory) method.")]
        public IHttpConnector Connector { get; set; }

        private Func<IHttpConnector> _createConnector;

        /// <summary>
        /// Merge the provided settings with the defaults
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        internal static HttpSettings Resolve(HttpSettings settings, HttpSettings defaults)
        {
            if (settings == null)
                return Default;
            var resolved = new HttpSettings
            {
                ContentType = settings.ContentType ?? defaults.ContentType,
                MemoryPool = settings.MemoryPool ?? defaults.MemoryPool,
                MaxResponseBufferSize = settings.MaxResponseBufferSize ?? defaults.MaxResponseBufferSize,
                ProtocolVersion = settings.ProtocolVersion ?? defaults.ProtocolVersion,
                TimeOut = settings.TimeOut ?? defaults.TimeOut,
                Serializer = settings.Serializer ?? defaults.Serializer,
                IsProxyDisabled = settings.IsProxyDisabled ?? defaults.IsProxyDisabled
            };

            // Late bind the memory pool so that it can use any of the resolved settings during construction.
            resolved.MemoryPool = settings.MemoryPool ?? defaults.MemoryPool;
            // Late bind the http connector so that it can use any of the resolved settings during construction.
#pragma warning disable CS0618 
            // This line is added to keep backward compatibility.
            resolved.Connector = settings.Connector ?? defaults.Connector;
#pragma warning restore CS0618 // 
            // Late bind the http connector factory so that it can use any of the resolved settings during construction.
            Func<IHttpConnector> defaultFactory = () => new WebRequestConnector(resolved.MemoryPool);
            resolved._createConnector = settings._createConnector ?? defaultFactory;
            MergeHeadersWithDefault(settings.Headers, defaults.Headers);
            settings.Headers.Keys.ToList().ForEach(key => resolved.Headers.Add(key, settings.Headers[key]));
            return resolved;
        }

        public IHttpConnector BuildConnector()
        {
            return _createConnector?.Invoke();
        }
        
        private static void MergeHeadersWithDefault(Dictionary<string, string> instanceHeaders, Dictionary<string, string> defaults)
        {
            if( defaults == null ) return;
            foreach( var header in defaults )
            {
                // Only add a global header if the instance header does not already contain it.
                if (instanceHeaders.ContainsKey(header.Key) == false)
                    instanceHeaders[header.Key] = header.Value;
            }
        }

        public HttpSettings WithMaxResponseBufferSize(long maxResponseBufferSize)
        {
            MaxResponseBufferSize = maxResponseBufferSize;
            return this;
        }

        public HttpSettings WithTimeOut(TimeSpan timeOut)
        {
            TimeOut = timeOut;
            return this;
        }

        [Obsolete("Use the WithConnector(Func<IHttpConnector> connectorFactory) overload.")]
        public HttpSettings WithConnector(IHttpConnector connector)
        {
            Connector = connector;
            Func<IHttpConnector> func = () => connector;
            _createConnector = func;
            return this;
        }

        public HttpSettings WithConnector(Func<IHttpConnector> connectorFactory)
        {
            _createConnector = connectorFactory;
            return this;
        }

        public HttpSettings WithSerializer(ISerializer serializer)
        {
            Serializer = serializer;
            return this;
        }

        public HttpSettings WithHeaders(NameValueCollection headers)
        {
            headers?.AllKeys.ToList().ForEach(h =>
            {
                Headers[h] = headers[h];
            });
            return this;
        }

        public HttpSettings WithHeader(string header, string value)
        {
            Headers[header] = value;
            return this;
        }

        public HttpSettings WithProtocolVersion(string protocolVersion)
        {
            ProtocolVersion = protocolVersion;
            return this;
        }

        public HttpSettings WithContentType(string contentType)
        {
            ContentType = contentType;
            return this;
        }

        public HttpSettings WithMemoryStreamPool(IMemoryStreamPool memoryStreamPool)
        {
            MemoryPool = memoryStreamPool;
            return this;
        }

        public HttpSettings WithDisabledProxy()
        {
            IsProxyDisabled = true;
            return this;
        }

    }
}
