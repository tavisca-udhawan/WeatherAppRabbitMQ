using System;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    public class CallerSetting
    {
        public Client Client { get; set; }
        public ClientSetting ClientSetting { get; set; }
        public object SerializerSetting { get; set; }
        public Serializer Serializer { get; set; }
        public Logger Logger { get; set; }
        public Type ErrorPayloadType { get; set; }
        public ErrorPayloadTypeSelector ErrorPayloadTypeSelector { get; set; }
    }
}
