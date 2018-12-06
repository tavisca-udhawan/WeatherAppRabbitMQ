using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.RecyclableStreamPool;
using Xunit;

namespace Tavisca.Platform.Common.Tests
{
    public class HttpClientConfiguratorTests
    {
        [Fact]
        public void TestHttpClientConfiguratorForDefaultConfigurations()
        {
            var settings = new HttpSettings();
            Assert.Equal(settings.ContentType, "application/json");
            Assert.Equal(settings.MaxResponseBufferSize, 65556);
            Assert.Equal(settings.TimeOut, new TimeSpan(0, 0, 60));
            Assert.Equal(settings.ProtocolVersion, "1.1");
            Assert.Empty(settings.Headers);
            Assert.Null(settings.Serializer);

            var contentType = "xml";
            var MaxBufferSize = 12345;
            var protocolVersion = "2.0";
            var timeOut = new TimeSpan(0, 0, 30);

            new HttpClientConfigurator(settings)
                .WithContentType(contentType)
                .WithHeader("key1", "val1")
                .WithMaxResponseBufferSize(MaxBufferSize)
                .WithMemoryStreamPool(new RecyclableStreamPool())
                .WithProtocolVersion(protocolVersion)
                .WithSerializer(HttpStub.CreateMockSerializer())
                .WithTimeOut(timeOut);

            Assert.Equal(settings.ContentType, contentType);
            Assert.Equal(settings.MaxResponseBufferSize, MaxBufferSize);
            Assert.Equal(settings.TimeOut?.TotalMilliseconds, timeOut.TotalMilliseconds);
            Assert.Equal(settings.ProtocolVersion, protocolVersion);
            Assert.NotEmpty(settings.Headers);
            Assert.NotNull(settings.Serializer);
            Assert.NotNull(settings.MemoryPool);
        }
    }
}
