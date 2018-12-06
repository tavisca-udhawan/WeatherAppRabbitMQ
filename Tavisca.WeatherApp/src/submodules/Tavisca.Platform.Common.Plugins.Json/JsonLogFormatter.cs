using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.MemoryStreamPool;
using RStream = Tavisca.Common.Plugins.RecyclableStreamPool.SynchronousRecyclableStreamPool;

namespace Tavisca.Platform.Common.Plugins.Json
{

    internal static class Globals
    {
        public static readonly ISynchronousMemoryStreamPool Buffer = new RStream();
    }

    public class JsonLogFormatter : ILogFormatter
    {
        public static readonly ILogFormatter Instance = new JsonLogFormatter();

        private JsonLogFormatter() { }

        private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);

        public byte[] Format(ILog log)
        {
            var fields = log.GetFields().ToList();
            using (var buffer = Globals.Buffer.GetMemoryStream())
            {
                using (var txtWriter = new StreamWriter(buffer, Utf8WithoutBom))
                {
                    using (var writer = new JsonTextWriter(txtWriter))
                    {
                        writer.WriteStartObject();
                        fields.ForEach(f => WriteFieldSafely(writer, f));
                        writer.WriteEndObject();
                        writer.Flush();
                        return buffer.ToArray();
                    }
                }
            }
        }

        private void WriteFieldSafely(JsonWriter writer, KeyValuePair<string, object> field)
        {
            try
            {
                var geoPoint = field.Value as GeoPoint;
                if (geoPoint != null)
                {
                    WriteValueAsGeo(writer, field.Key, geoPoint);
                    return;
                }

                var payload = field.Value as Payload;
                if (payload != null)
                {
                    WriteValueAsBinary(writer, field.Key, payload);
                    return;
                }

                var map = field.Value as Map;
                if (map != null)
                {
                    WriteValueAsMap(writer, field.Key, map);
                    return;
                }

                var ipAddress = field.Value as IPAddress;
                if (ipAddress != null)
                {
                    WriteValueAsIp(writer, field.Key, ipAddress);
                    return;
                }

                WriteDefault(writer, field.Key, field.Value);
            }
            catch
            {
                // ignored
            }
        }

        private void WriteValueAsIp(JsonWriter writer, string key, IPAddress value)
        {
            writer.WritePropertyName("ip_" + key);
            writer.WriteValue(value.ToString());
        }

        private void WriteValueAsGeo(JsonWriter writer, string name, GeoPoint geo)
        {
            // Geo points need to be prefixed with geo_.
            writer.WritePropertyName("geo_" + name);
            writer.WriteStartObject();
            writer.WritePropertyName("lat");
            writer.WriteValue(geo.Latitude);
            writer.WritePropertyName("lon");
            writer.WriteValue(geo.Longitude);
            writer.WriteEndObject();
        }

        private void WriteValueAsMap(JsonWriter writer, string name, Map map)
        {
            if (map?.Value?.Count > 0)
            {
                if (map.Format == MapFormat.Json)
                {
                    // Map fields need to be prefixed with json_
                    writer.WritePropertyName("json_" + name);

                    writer.WriteStartObject();
                    foreach (var item in map.Value)
                    {
                        writer.WritePropertyName(item.Key);
                        writer.WriteValue(item.Value);
                    }
                    writer.WriteEndObject();
                }
                else
                {
                    writer.WritePropertyName(name);
                    var buffer = new StringBuilder();
                    foreach (var item in map.Value)
                    {
                        if (buffer.Length == 0)
                            buffer.Append($"{item.Key}={item.Value}");
                        else
                            buffer.Append($"|{item.Key}={item.Value}");
                    }
                    writer.WriteValue(buffer.ToString());
                }
            }
        }

        private void WriteValueAsBinary(JsonWriter writer, string name, Payload payload)
        {
            var value = payload.GetBytes();
            string base64Value = null;
            if (value.Length > 0)
            {
                byte[] compressed = null;
                // Compress and then encode the value
                using (var buffer = new MemoryStream())
                {
                    using (var gzip = new GZipStream(buffer, CompressionMode.Compress, true))
                    {
                        gzip.Write(value, 0, value.Length);
                    }
                    compressed = buffer.ToArray();
                }
                base64Value = Convert.ToBase64String(compressed);
            }
            // Byte fields need to be prefixed with stream_.
            writer.WritePropertyName("binary_" + name);
            writer.WriteValue(base64Value);

        }

        private void WriteDefault(JsonWriter writer, string name, object value)
        {
            writer.WritePropertyName(name);
            if (value is decimal)
            {
                writer.WriteValue(Convert.ToDouble(value));
                return;
            }
            writer.WriteValue(value);
        }
    }
}
