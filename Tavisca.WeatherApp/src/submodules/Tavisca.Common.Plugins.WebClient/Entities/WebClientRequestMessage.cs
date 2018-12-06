using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.WebClient
{
    public class WebClientRequestMessage
    {
        public string Url { get; set; }

        public byte[] Data { get; set; }

        public NameValueCollection RequestHeaders { get; private set; }

        public NameValueCollection ContentHeaders { get; private set; }

        public WebClientRequestMessage()
        {
            RequestHeaders = new NameValueCollection();
            ContentHeaders = new NameValueCollection();
        }

        public async Task<HttpContent> GetHttpContentAsync()
        {
            if (Data == null)
                return null;

            using (var stream = new MemoryStream(Data))
            {
                var httpContent = new StreamContent(stream);
                await httpContent.LoadIntoBufferAsync();
                foreach (var key in ContentHeaders.AllKeys)
                {
                    httpContent.Headers.Add(key, ContentHeaders[key]);
                }
                return httpContent;
            }
        }
    }
}
