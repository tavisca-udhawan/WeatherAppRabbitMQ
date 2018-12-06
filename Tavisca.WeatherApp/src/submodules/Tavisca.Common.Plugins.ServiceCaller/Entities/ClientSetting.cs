using System;
using System.Collections.Specialized;
using System.Text;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    public class ClientSetting
    {
        public long MaxResponseBufferSize { get; private set; }
        public TimeSpan TimeOut { get; private set; }
        public NameValueCollection Headers { get; private set; }
        public Encoding Encoding { get; private set; }
        public string ContentType { get; private set; }
        public HttpCompletionOption HttpCompletionOption { get; private set; }
        public ClientSetting(TimeSpan timeOut, NameValueCollection defaultHeaders = null, string contentType = "application/json", long maxBufferSize = int.MaxValue, Encoding encoding = null, HttpCompletionOption httpCompletionOption = HttpCompletionOption.ResponseContentRead)
        {
            MaxResponseBufferSize = maxBufferSize;
            TimeOut = timeOut;
            Headers = defaultHeaders ?? new NameValueCollection();
            ContentType = contentType;
            HttpCompletionOption = httpCompletionOption;
            Encoding = encoding??Encoding.UTF8;
        }

    }
}
