using Microsoft.IO;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.MemoryStreamPool;

namespace Tavisca.Common.Plugins.RecyclableStreamPool
{
    public class RecyclableStreamPool : IMemoryStreamPool
    {
        private static readonly RecyclableMemoryStreamManager _streamManager = new RecyclableMemoryStreamManager();
        public Task<MemoryStream> GetMemoryStream(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(_streamManager.GetStream());
        }

        public async Task<MemoryStream> GetMemoryStream(byte[] data, CancellationToken cancellationToken = default(CancellationToken))
        {
            var stream = _streamManager.GetStream();
            await stream.WriteAsync(data, 0, data.Length, cancellationToken);
            stream.Position = 0;
            return stream;
        }
    }
}
