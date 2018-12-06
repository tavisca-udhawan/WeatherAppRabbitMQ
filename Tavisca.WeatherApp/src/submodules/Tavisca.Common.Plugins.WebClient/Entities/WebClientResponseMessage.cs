using System.Collections.Specialized;
using System.Net;

namespace Tavisca.Common.Plugins.WebClient
{
    public class WebClientResponseMessage
    {

        public byte[] Data { get; set; }
        public HttpStatusCode HttpStatusCode { get;private set; }

        public NameValueCollection ResponseHeaders { get; private set; }

        public NameValueCollection ContentHeaders { get; private set; }

        public WebClientResponseMessage(HttpStatusCode httpStatusCode)
        {
            HttpStatusCode = httpStatusCode;
            ResponseHeaders = new NameValueCollection();
            ContentHeaders= new NameValueCollection();
        }

    }
}
