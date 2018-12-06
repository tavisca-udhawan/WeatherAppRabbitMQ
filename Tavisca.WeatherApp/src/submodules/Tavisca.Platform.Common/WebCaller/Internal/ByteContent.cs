using System.Threading.Tasks;

namespace Tavisca.Platform.Common
{
    public class ByteContent : IContent
    {
        public ByteContent(byte[] payload)
        {
            Payload = payload;
        }

        public byte[] Payload { get; }

        public Task<byte[]> GetPayloadAsync(HttpSettings settings)
        {
            return Task.FromResult(Payload);
        }
    }
}
