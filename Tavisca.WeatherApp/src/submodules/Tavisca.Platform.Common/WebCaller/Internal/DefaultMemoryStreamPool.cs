using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.MemoryStreamPool;

namespace Tavisca.Platform.Common
{
    public class DefaultMemoryStreamPool : IMemoryStreamPool
    {
        public Task<MemoryStream> GetMemoryStream(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(new MemoryStream());
        }

        public async Task<MemoryStream> GetMemoryStream(byte[] data, CancellationToken cancellationToken = default(CancellationToken))
        {
            var stream = new MemoryStream();
            await stream.WriteAsync(data, 0, data.Length, cancellationToken);
            stream.Position = 0;
            return stream;
        }
    }
}
