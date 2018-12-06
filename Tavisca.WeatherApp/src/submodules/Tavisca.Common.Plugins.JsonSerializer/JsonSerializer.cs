using System;
using System.IO;
using System.Text;
using Tavisca.Platform.Common.Serialization;

namespace Tavisca.Common.Plugins.JsonSerializer
{
    public class JsonSerializer : ISerializer
    {
        private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);
        private const int _streamBuffer = 1024;
        public void Serialize<T>(Stream outputStream, T data)
        {
            var formatter = new Newtonsoft.Json.JsonSerializer();
            using (var writer = new StreamWriter(outputStream, Utf8WithoutBom, _streamBuffer, true))
                formatter.Serialize(writer, data);
        }

        public T Deserialize<T>(Stream inputStream)
        {
            var formatter = new Newtonsoft.Json.JsonSerializer();
            using (var reader = new StreamReader(inputStream, Utf8WithoutBom, true, _streamBuffer, true))
                return (T)formatter.Deserialize(reader, typeof(T));
        }

        public void Serialize(Stream outputStream, object data, Type type = null)
        {
            var formatter = new Newtonsoft.Json.JsonSerializer();
            using (var writer = new StreamWriter(outputStream, Utf8WithoutBom, _streamBuffer, true))
                formatter.Serialize(writer, data);
        }

        public object Deserialize(Stream inputStream, Type type)
        {
            var formatter = new Newtonsoft.Json.JsonSerializer();
            using (var reader = new StreamReader(inputStream, Utf8WithoutBom, true, _streamBuffer, true))
                return formatter.Deserialize(reader, type);
        }
    }
}
