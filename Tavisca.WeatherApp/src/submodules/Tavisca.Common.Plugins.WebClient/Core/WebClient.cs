using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.WebClient
{
    public class WebClient : IWebClient
    {
        private System.Net.Http.HttpClient _httpClient;

        internal WebClient()
        {
            var handler = new HttpClientHandler() {AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate };
            _httpClient = new HttpClient(handler);
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
        }

        internal WebClient(int timeOut): this()
        {
            _httpClient.Timeout = new TimeSpan(0, 0, 0, 0,timeOut);
        }

        public async Task<WebClientResponseMessage> GetAsync(WebClientRequestMessage request, CancellationToken cancellationToken= default(CancellationToken))
        {
            var httpRequestMessage = await CreateHttpRequestMessage(request, new System.Net.Http.HttpMethod(Constants.HttpMethod.GET));

            var result = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

            return await CreateWebClientResponseMessage(result);

        }

        public async Task<WebClientResponseMessage> PostAsync(WebClientRequestMessage request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var httpRequestMessage = await CreateHttpRequestMessage(request, new System.Net.Http.HttpMethod(Constants.HttpMethod.POST));

            var result = await _httpClient.SendAsync(httpRequestMessage,cancellationToken);

            return await CreateWebClientResponseMessage(result);
        }

        public async Task<WebClientResponseMessage> PutAsync(WebClientRequestMessage request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var httpRequestMessage = await CreateHttpRequestMessage(request, new System.Net.Http.HttpMethod(Constants.HttpMethod.PUT));

            var result = await _httpClient.SendAsync(httpRequestMessage,cancellationToken);

            return await CreateWebClientResponseMessage(result);
        }

        public async Task<WebClientResponseMessage> DeleteAsync(WebClientRequestMessage request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var httpRequestMessage = await CreateHttpRequestMessage(request, new System.Net.Http.HttpMethod(Constants.HttpMethod.DELETE));

            var result = await _httpClient.SendAsync(httpRequestMessage,cancellationToken);

            return await CreateWebClientResponseMessage(result);
        }

        public async Task<WebClientResponseMessage> PatchAsync(WebClientRequestMessage request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var httpRequestMessage = await CreateHttpRequestMessage(request, new System.Net.Http.HttpMethod(Constants.HttpMethod.PATCH));

            var result = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

            return await CreateWebClientResponseMessage(result);
        }


        private static async Task<HttpRequestMessage> CreateHttpRequestMessage(WebClientRequestMessage message, System.Net.Http.HttpMethod httpMethod)
        {
            var httpRequestMessage = new HttpRequestMessage(httpMethod, message.Url);
            httpRequestMessage.Content = await message.GetHttpContentAsync();

            foreach (var key in message.RequestHeaders.AllKeys)
            {
                httpRequestMessage.Headers.Add(key, message.RequestHeaders[key]);
            }
            return httpRequestMessage;
        }

        private static async Task<WebClientResponseMessage> CreateWebClientResponseMessage(HttpResponseMessage httpResponseMessage)
        {
            // Read Data
            var dataTask = httpResponseMessage.Content.ReadAsByteArrayAsync();

            // Create response message
            var response = new WebClientResponseMessage(httpResponseMessage.StatusCode);
            response.ResponseHeaders.Add(httpResponseMessage.GetResponseHeaders());
            response.ContentHeaders.Add(httpResponseMessage.GetResponseContentHeaders());

            // Set data
            var data = await dataTask;
            response.Data = data;

            return response;
        }

        public void Dispose()
        {
            if (_httpClient != null)
                _httpClient.Dispose();
        }
    }
}
