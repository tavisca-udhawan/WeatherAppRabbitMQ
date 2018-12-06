using System.Threading.Tasks;

namespace Tavisca.Platform.Common
{
    public class ObjectContent<T> : IContent
    {
        public ObjectContent(T obj)
        {
            Object = obj;
        }

        public T Object { get; }

        private byte[] _cachedPayload;

        public async Task<byte[]> GetPayloadAsync(HttpSettings settings)
        {
            if (_cachedPayload != null)
                return _cachedPayload;
            using (var stream = await settings.MemoryPool.GetMemoryStream())
            {
                settings.Serializer.Serialize(stream, Object);
                var payload = stream.ToArray();
                //cache the serialized object so that serialization is not executed on repeated calls
                _cachedPayload = payload;
                return payload;
            }
        }
    }
}
