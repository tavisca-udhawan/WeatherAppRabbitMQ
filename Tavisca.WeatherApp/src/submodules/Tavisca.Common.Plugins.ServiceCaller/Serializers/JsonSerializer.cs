using System;
#if NET_STANDARD
using Tavisca.Frameworks.Serialization.netCore.SerializerFacade;
#else
using Tavisca.Frameworks.Serialization.Binary;
#endif
using Tavisca.Platform.Common.Profiling;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    internal class JsonSerializer : Serializer
    {
        private readonly JsonNetSerializerFacade _jsonSerializer;
        public JsonSerializer()
        {
            _jsonSerializer = new JsonNetSerializerFacade();
        }

        public override T DeepClone<T>(T obj)
        {
            throw new NotImplementedException();
        }

        public override T Deserialize<T>(byte[] data, object deserializationSetting = null)
        {
            using (new ProfileContext("jsonSerializer-deSerialize"))
                return _jsonSerializer.Deserialize<T>(data, deserializationSetting);
        }

        public override byte[] Serialize(object obj, object serializationSetting = null)
        {
            using (new ProfileContext("jsonSerializer-serialize"))
                return _jsonSerializer.Serialize(obj, serializationSetting);
        }
    }

}
