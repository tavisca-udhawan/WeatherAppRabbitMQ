using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.WebClient
{
    public static class Extensions
    {
        internal static NameValueCollection GetResponseHeaders(this HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage == null || httpResponseMessage.Headers == null)
                return new NameValueCollection();
            NameValueCollection headers = new NameValueCollection();
            httpResponseMessage.Headers.ToList().ForEach(x => headers.Add(x.Key, x.Value.First()));
            return headers;
        }

        internal static NameValueCollection GetResponseContentHeaders(this HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage == null || httpResponseMessage.Content.Headers == null)
                return new NameValueCollection();
            NameValueCollection headers = new NameValueCollection();
            httpResponseMessage.Content.Headers.ToList().ForEach(x => headers.Add(x.Key, x.Value.First()));
            return headers;
        }

        public static async Task<WebClientResponseMessage> GetAsync(this IWebClient client, WebClientRequestMessage request)
        {
            return await client.GetAsync(request, CancellationToken.None);
        }

        public static async Task<WebClientResponseMessage> PatchAsync(this IWebClient client, WebClientRequestMessage request)
        {
            return await client.PatchAsync(request, CancellationToken.None);
        }

        public static async Task<WebClientResponseMessage> PostAsync(this IWebClient client, WebClientRequestMessage request)
        {
            return await client.PostAsync(request, CancellationToken.None);
        }

        public static async Task<WebClientResponseMessage> PutAsync(this IWebClient client, WebClientRequestMessage request)
        {
            return await client.PutAsync(request, CancellationToken.None);
        }

        public static async Task<WebClientResponseMessage> DeleteAsync(this IWebClient client, WebClientRequestMessage request)
        {
            return await client.DeleteAsync(request, CancellationToken.None);
        }
    }
}
