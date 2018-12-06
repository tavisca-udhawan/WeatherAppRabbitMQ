using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common
{
    public static class Http
    {
        static Http()
        {
#if !NET_STANDARD
            // Added support for connecting services supporting TLS 1.1 and 1.2(Outbound calls). This line will not override the previous value, but will add the following.
            // By default microsoft supports SSL3 and TLS.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
#endif
        }

        /// <summary>
        /// Use this method to statically configure the default settings for the web caller.
        /// This should typically be called once when the application starts.
        /// </summary>
        /// <returns></returns>
        public static HttpClientConfigurator ConfigureDefaults()
        {
            return new HttpClientConfigurator(HttpSettings.Default);
        }

        public static HttpRequest NewPostRequest(Uri uri, HttpSettings settings = null)
        {
            return new HttpRequest(uri, settings)
            {
                Method = HttpMethods.POST
            };
        }

        public static HttpRequest NewGetRequest(Uri uri, HttpSettings settings = null)
        {
            return new HttpRequest(uri, settings)
            {
                Method = HttpMethods.GET
            };
        }

        public static HttpRequest NewDeleteRequest(Uri uri, HttpSettings settings = null)
        {
            return new HttpRequest(uri, settings)
            {
                Method = HttpMethods.DELETE
            };
        }

        public static HttpRequest NewPutRequest(Uri uri, HttpSettings settings = null)
        {
            return new HttpRequest(uri, settings)
            {
                Method = HttpMethods.PUT
            };
        }

        public static HttpRequest NewRequest(string method, Uri uri, HttpSettings settings = null)
        {
            return new HttpRequest(uri, settings)
            {
                Method = method
            };
        }
    }
}
