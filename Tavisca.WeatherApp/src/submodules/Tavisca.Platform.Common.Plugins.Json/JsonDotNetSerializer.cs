using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Converters;
using Tavisca.Platform.Common.Profiling;
using Tavisca.Platform.Common.Serialization;
using System.Text;

namespace Tavisca.Platform.Common.Plugins.Json
{
    public class JsonDotNetSerializer : ISerializer
    {
        private static readonly Encoding UTF8WithoutBom = new UTF8Encoding(false);
        private const int _streamBuffer = 1024;
        public JsonDotNetSerializer(ITranslatorMapping mapping)
        {
            var formatter = new JsonSerializer();
            formatter.Converters.Add(new StringEnumConverter());
            formatter.ContractResolver = new MappedTypeContractResolver(mapping);
            _formatter = formatter;
        }

        public JsonDotNetSerializer(ITranslatorMapping mapping, List<Tuple<Type, JsonConverter>> customConverters, bool isCamelCaseText = false)
        {
            var formatter = new JsonSerializer();
            formatter.Converters.Add(new StringEnumConverter(isCamelCaseText));
            formatter.ContractResolver = new MappedTypeContractResolver(mapping, customConverters);
            _formatter = formatter;
        }

        public JsonDotNetSerializer(JsonSerializer formatter)
        {
            _formatter = formatter;
        }

        private readonly JsonSerializer _formatter;
        public T Deserialize<T>(Stream inputStream)
        {
            using (new ProfileContext("jsonDotNetSerializer-deSerialize"))
            using (var reader = new StreamReader(inputStream, UTF8WithoutBom, true, _streamBuffer, true))
                return (T)_formatter.Deserialize(reader, typeof(T));
        }

        public void Serialize<T>(Stream outputStream, T data)
        {
            using (new ProfileContext("jsonDotNetSerializer-serialize"))
            using (var writer = new StreamWriter(outputStream, UTF8WithoutBom, _streamBuffer, true))
                _formatter.Serialize(writer, data);
        }

        public void Serialize(Stream outputStream, object data, Type type = null)
        {
            using (new ProfileContext("jsonDotNetSerializer-serialize"))
            using (var writer = new StreamWriter(outputStream, UTF8WithoutBom, _streamBuffer, true))
                _formatter.Serialize(writer, data, type);
        }

        public object Deserialize(Stream inputStream, Type type)
        {
            using (new ProfileContext("jsonDotNetSerializer-deSerialize"))
            using (var reader = new StreamReader(inputStream, UTF8WithoutBom, true, _streamBuffer, true))
                return _formatter.Deserialize(reader, type);
        }
    }
}
