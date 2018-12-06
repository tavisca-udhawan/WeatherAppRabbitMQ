using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.MemoryStreamPool
{
    public interface IMemoryStreamPool
    {
        Task<MemoryStream> GetMemoryStream(CancellationToken cancellationToken = default(CancellationToken));
        Task<MemoryStream> GetMemoryStream(byte[] data, CancellationToken cancellationToken = default(CancellationToken));
    }

    // Ideally these methods should be part of IMemoryStreamPool. However, the naming convention does not allow for this.
    public interface ISynchronousMemoryStreamPool
    {
        MemoryStream GetMemoryStream();
        MemoryStream GetMemoryStream(byte[] data);
    }
}
