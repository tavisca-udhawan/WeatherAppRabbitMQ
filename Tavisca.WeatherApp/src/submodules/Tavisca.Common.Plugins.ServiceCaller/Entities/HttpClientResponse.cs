using System.Net;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    public class HttpClientResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string Content { get; set; }
    }
}
