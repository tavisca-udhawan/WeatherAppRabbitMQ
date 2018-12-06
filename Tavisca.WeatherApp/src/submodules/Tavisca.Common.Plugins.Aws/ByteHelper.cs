using System.IO;
using System.IO.Compression;

namespace Tavisca.Common.Plugins.Aws
{
    internal static class ByteHelper
    {
        public static MemoryStream Compress(byte[] data)
        {
            var buffer = new MemoryStream();
            using (var gzip = new GZipStream(buffer, CompressionMode.Compress, true))
            {
                gzip.Write(data, 0, data.Length);
            }
            buffer.Position = 0;
            return buffer;
        }
    }
}
