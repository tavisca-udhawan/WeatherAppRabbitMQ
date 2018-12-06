using Newtonsoft.Json;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    public interface IRequest
    {
        ClientSetting ClientSetting { get; set; }
        ApiEndPoint EndPoint { get; set; }
        JsonSerializerSettings SerializerSettings { get; set; }
    }
}
