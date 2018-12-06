using Microsoft.IO;
using System.IO;
using Tavisca.Platform.Common.MemoryStreamPool;

namespace Tavisca.Common.Plugins.RecyclableStreamPool
{
    public class SynchronousRecyclableStreamPool : ISynchronousMemoryStreamPool
    {
        private static readonly RecyclableMemoryStreamManager StreamManager = new RecyclableMemoryStreamManager();
        public MemoryStream GetMemoryStream()
        {
            return StreamManager.GetStream();
        }

        public MemoryStream GetMemoryStream(byte[] data)
        {
            var stream = StreamManager.GetStream();
            stream.Write(data, 0, data.Length);
            stream.Position = 0;
            return stream;
        }
    }
}
