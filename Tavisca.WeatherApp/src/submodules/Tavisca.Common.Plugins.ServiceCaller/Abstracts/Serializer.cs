#if NET_STANDARD
using Tavisca.Frameworks.Serialization.netCore;
#else
using Tavisca.Frameworks.Serialization;
#endif
namespace Tavisca.Common.Plugins.ServiceCaller
{
    public abstract class Serializer : ISerializationFacade
    {
        public abstract T DeepClone<T>(T obj);
        public abstract T Deserialize<T>(byte[] data, object deserializationSetting = null);
        public abstract byte[] Serialize(object obj, object serializationSetting = null);
        public virtual string GetStringFromBytes(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

    }
}
