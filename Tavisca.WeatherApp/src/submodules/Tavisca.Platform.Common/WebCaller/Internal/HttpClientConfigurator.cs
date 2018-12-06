using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using Tavisca.Platform.Common.MemoryStreamPool;
using Tavisca.Platform.Common.Serialization;

namespace Tavisca.Platform.Common
{
    public class HttpClientConfigurator
    {
        public HttpClientConfigurator(HttpSettings settings)
        {
            _settings = settings;
        }

        private HttpSettings _settings;

        public HttpClientConfigurator WithMemoryStreamPool(IMemoryStreamPool memoryStreamPool)
        {
            _settings.WithMemoryStreamPool(memoryStreamPool);
            return this;
        }

        public HttpClientConfigurator WithSerializer(ISerializer serializer)
        {
            _settings.WithSerializer(serializer);
            return this;
        }

        public HttpClientConfigurator WithHttpConnector(IHttpConnector connector)
        {
            Func<IHttpConnector> func = () => connector;
            _settings.WithConnector(func);
            return this;
        }

        public HttpClientConfigurator WithMaxResponseBufferSize(long maxResponseBufferSize)
        {
            _settings.WithMaxResponseBufferSize(maxResponseBufferSize);
            return this;
        }

        public HttpClientConfigurator WithTimeOut(TimeSpan timeOut)
        {
            _settings.WithTimeOut(timeOut);
            return this;
        }

        public HttpClientConfigurator WithHeader(string name, string value)
        {
            _settings.WithHeader(name, value);
            return this;
        }

        public HttpClientConfigurator WithProtocolVersion(string protocolVersion)
        {
            _settings.WithProtocolVersion(protocolVersion);
            return this;
        }

        public HttpClientConfigurator WithContentType(string contentType)
        {
            _settings.WithContentType(contentType);
            return this;
        }

        public HttpClientConfigurator WithDisabledProxy()
        {
            _settings.WithDisabledProxy();
            return this;
        }
    }
}