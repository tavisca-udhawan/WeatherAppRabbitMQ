using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Tavisca.Common.Plugins.RecyclableStreamPool;
using Tavisca.Platform.Common.MemoryStreamPool;

namespace Tavisca.Platform.Common.Plugins.Json
{
    public static class ByteHelper
    {
        public static readonly ISynchronousMemoryStreamPool Buffer = new SynchronousRecyclableStreamPool();
        private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);

        public static Func<byte[]> ToByteArrayUsingJsonSerialization(object obj, Encoding encoding = null)
        {
            return () => ConvertToByteArray(obj, encoding ?? Utf8WithoutBom);
        }

        private static byte[] ConvertToByteArray(object obj, Encoding encoding)
        {
            byte[] bytes;
            using (var buffer = Buffer.GetMemoryStream())
            {
                using (var txtWriter = new StreamWriter(buffer, encoding))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(txtWriter, obj);

                    txtWriter.Flush();

                    bytes = buffer.ToArray();

                    return bytes;
                }
            }
        }
    }
}