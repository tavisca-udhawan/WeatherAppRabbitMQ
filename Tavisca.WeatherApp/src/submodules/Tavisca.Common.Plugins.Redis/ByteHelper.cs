using System;
using System.IO;
using System.IO.Compression;

namespace Tavisca.Common.Plugins.Redis
{
    internal static class ByteHelper
    {
        public static string CompressAndEncode(byte[] data)
        {
            using (var buffer = new MemoryStream())
            {
                using (var gzip = new GZipStream(buffer, CompressionMode.Compress, true))
                {
                    gzip.Write(data, 0, data.Length);
                }
                return Convert.ToBase64String(buffer.ToArray()); 
            }
        }
    }
}
