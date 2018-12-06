using System.Threading.Tasks;

namespace Tavisca.Platform.Common
{
    public interface IContent
    {
        Task<byte[]> GetPayloadAsync(HttpSettings settings);
    }
}
