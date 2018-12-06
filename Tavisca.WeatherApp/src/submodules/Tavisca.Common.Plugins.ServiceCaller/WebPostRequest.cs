using Newtonsoft.Json;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    public class WebPostRequest<T> : IRequest
    {
        public T Request { get; set; }
        public ClientSetting ClientSetting { get; set; }
        public ApiEndPoint EndPoint { get; set; }
        public JsonSerializerSettings SerializerSettings { get; set; }
        
    }
}
