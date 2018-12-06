using Newtonsoft.Json;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    public class WebGetRequest : IRequest
    {
        public ClientSetting ClientSetting { get; set; }
        public ApiEndPoint EndPoint { get; set; }
        public JsonSerializerSettings SerializerSettings { get; set; }
    }
}
