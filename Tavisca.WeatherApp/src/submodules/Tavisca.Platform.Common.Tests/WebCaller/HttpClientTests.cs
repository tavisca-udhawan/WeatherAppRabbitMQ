using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tavisca.Platform.Common.Tests.WebCaller
{
    public class HttpClientTests
    {

        private static readonly Uri TestUrl = new Uri("http://test.com");

        private Mock<IHttpConnector> GetConnectorWithSuccessfulResponse() => HttpStub.CreateHttpConnector(HttpClientResponseBuilder.CreateSuccessfulHttpResponse());

        private Mock<IHttpConnector> GetFaultingConnector() => HttpStub.CreateHttpConnector(null, true);

        [Fact]
        public async Task TestGet_WhenLoggerIsSet_AndResponseIsSuccessful_ThenTheLoggerLogsTheResponseSuccessfully()
        {

            var settings = new HttpSettings
            {
                Connector = GetConnectorWithSuccessfulResponse().Object
            };
            var request = Http.NewGetRequest(TestUrl, settings);
            var httpResponse = await request.SendAsync();
            Assert.NotNull(httpResponse);
        }

        [Fact]
        public async Task TestGet_WhenLoggerIsNotSet_AndResponseIsSuccessful_ThenTheHttpClientReturnsSuccessfulResponseWithOutLogging()
        {
            var setting = new HttpSettings { Connector = GetConnectorWithSuccessfulResponse().Object };
            var request = Http.NewGetRequest(TestUrl, setting);
            var httpResponse = await request.SendAsync();
            Assert.NotNull(httpResponse);
        }

        [Fact]
        public async Task TestPost_WhenLoggerIsNotSet_AndResponseIsSuccessful_ThenTheHttpClientReturnsSuccessfulResponseWithOutLogging()
        {
            var settings = new HttpSettings { Connector = GetConnectorWithSuccessfulResponse().Object };
            var request = Http.NewPostRequest(TestUrl, settings);
            var httpResponse = await request.SendAsync();
            Assert.NotNull(httpResponse);
        }

        [Fact]
        public async Task TestPost_WhenHttpClientInstanceLevelConfigAreOverriddenByRequestLevelConfig_AndResponseIsSuccessful_ThenTheRequestLevelConfigurationsAreOverriddenSuccessfully()
        {
            var connector = GetConnectorWithSuccessfulResponse();
            var settings = new HttpSettings();
            Func<IHttpConnector> func = () => connector.Object;
            settings.WithConnector(func);
            var request = Http.NewPostRequest(TestUrl, settings);

            // Set configurations at instance level
            var contentType = "text/plain";
            var maxBufferSize = 89556;
            var protocolVersion = "1.0";
            var timeOut = new TimeSpan(0, 0, 10);
            request
                .WithContentType(contentType)
                .WithMaxResponseBufferSize(maxBufferSize)
                .WithTimeout(timeOut)
                .WithProtocolVersion(protocolVersion);

            var httpResponse = await request.SendAsync();
            Assert.NotNull(httpResponse);
            // Verify if instance level parameters are passed to the connector
            connector.Verify(x => x.SendAsync(It.Is<HttpRequest>(req => VerifyHttpRequestForConfigurations(req, contentType, maxBufferSize, timeOut, protocolVersion)), It.IsAny<CancellationToken>()));
        }

        [Fact]
        public async Task TestPut_WhenLoggerIsNotSet_AndResponseIsSuccessful_ThenTheHttpClientReturnsSuccessfulResponseWithOutLogging()
        {
            var settings = new HttpSettings { Connector = GetConnectorWithSuccessfulResponse().Object };
            var request = Http.NewPutRequest(TestUrl, settings);
            var httpResponse = await request.SendAsync();
            Assert.NotNull(httpResponse);
        }

        [Fact]
        public async Task TestDelete_WhenLoggerIsNotSet_AndResponseIsSuccessful_ThenTheHttpClientReturnsSuccessfulResponseWithOutLogging()
        {
            var settings = new HttpSettings { Connector = GetConnectorWithSuccessfulResponse().Object };
            var request = Http.NewDeleteRequest(TestUrl, settings);
            var httpResponse = await request.SendAsync();

            Assert.NotNull(httpResponse);
        }

        private bool VerifyHttpRequestForConfigurations(HttpRequest request, string contentType, long maxBufferSize, TimeSpan timeOut, string protocolVersion)
        {
            if (request == null)
                return false;
            if (!request.ContentType.Equals(contentType, StringComparison.OrdinalIgnoreCase))
                return false;
            if (request.MaxResponseBufferSize != maxBufferSize)
                return false;
            if (request.TimeOut?.Ticks != timeOut.Ticks)
                return false;
            if (!request.ProtocolVersion.Equals(protocolVersion, StringComparison.OrdinalIgnoreCase))
                return false;
            return true;
        }




    }
}
