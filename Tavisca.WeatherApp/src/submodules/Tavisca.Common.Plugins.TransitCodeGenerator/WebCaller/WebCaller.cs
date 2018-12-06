using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;
using Tavisca.Platform.Common;
using Tavisca.Platform.Common.Plugins.Json;


namespace Tavisca.Common.Plugins.TransitCodeGenerator
{
    class WebCaller
    {
        private HttpSettings _httpSettings = new HttpSettings();
        internal async Task<HttpRequest> GetWebRequest<T>(T request, string url)
        {
            return await CreatePostRequestAsync(request, new Uri(url), "transit_service", "generate_transit_code");
        }
        internal async Task<HttpRequest> CreatePostRequestAsync<T>(T request, Uri uri, string api, string verb)
        {
            var jsonSerializer = new Newtonsoft.Json.JsonSerializer() { TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Auto, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore, ContractResolver = new CamelCasePropertyNamesContractResolver() };
            jsonSerializer.Converters.Add(new StringEnumConverter());
            _httpSettings.WithSerializer(new JsonDotNetSerializer(jsonSerializer));
            _httpSettings.WithConnector(new WebRequestConnector());

            var req = Http.NewPostRequest(uri, _httpSettings)
                            .WithLogData("api", api)
                            .WithLogData("verb", verb);
            await req.SetBodyAsync(request);
            return req;
        }
    }
}
