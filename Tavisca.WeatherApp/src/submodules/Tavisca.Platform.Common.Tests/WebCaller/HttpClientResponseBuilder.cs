using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.RecyclableStreamPool;

namespace Tavisca.Platform.Common.Tests.WebCaller
{
    public class HttpClientResponseBuilder
    {
        public static HttpResponse CreateSuccessfulHttpResponse()
        {
            var requestPayload = new byte[1] { 56 };
            return new HttpResponse(System.Net.HttpStatusCode.OK, requestPayload);
        }

        


    }
}
