using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Plugins.Json;
using Xunit;

namespace Tavisca.Platform.Common.Tests.Logging
{
    public class JsonLogFormatterFixture
    {
        private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);

        [Fact]
        public void FormatterTest()
        {
            var id = Guid.NewGuid().ToString();
            var logTime = DateTime.Now;
            var fields = new List<KeyValuePair<string, object>>
            {
                new KeyValuePair<string, object>("string_field", id),
                new KeyValuePair<string, object>("datetime_field", logTime),
                new KeyValuePair<string, object>("int_field", int.MaxValue),
                new KeyValuePair<string, object>("uint_field", uint.MaxValue),
                new KeyValuePair<string, object>("long_field", long.MaxValue),
                new KeyValuePair<string, object>("ulong_field", ulong.MaxValue),
                new KeyValuePair<string, object>("boolean_field", true),
                new KeyValuePair<string, object>("float_field", float.MaxValue),
                new KeyValuePair<string, object>("double_field", double.MaxValue),
                new KeyValuePair<string, object>("decimal_field", decimal.MaxValue),
                new KeyValuePair<string, object>("geopoint_field", new GeoPoint(90.2m, 180.1m)),
                new KeyValuePair<string, object>("map_delimited_string_field", new Map(new Dictionary<string, string>
                {
                    {"field1", "value1"},
                    {"field2", "value2"}
                }, MapFormat.DelimitedString)),
                new KeyValuePair<string, object>("map_json_field", new Map(new Dictionary<string, string>
                {
                    {"field3", "value3"},
                    {"field4", "value4"}
                }, MapFormat.Json)),
                new KeyValuePair<string, object>("payload", new Payload("payload_string", Encoding.UTF8)),
                new KeyValuePair<string, object>("ipAddr", IPAddress.Loopback)
            };

            var log = new SimpleLog(id, logTime, fields);

            var byteData = JsonLogFormatter.Instance.Format(log);

            var jsonString = Utf8WithoutBom.GetString(byteData);
            var json = JObject.Parse(jsonString);

            Assert.Equal(id, json["string_field"].Value<string>());
            Assert.Equal(logTime, json["datetime_field"].Value<DateTime>());
            Assert.Equal(int.MaxValue, json["int_field"].Value<int>());
            Assert.Equal(uint.MaxValue, json["uint_field"].Value<uint>());
            Assert.Equal(long.MaxValue, json["long_field"].Value<long>());
            Assert.Equal(ulong.MaxValue, (ulong)json["ulong_field"]);
            Assert.Equal(true, json["boolean_field"].Value<bool>());
            Assert.Equal(float.MaxValue, json["float_field"].Value<float>());
            Assert.Equal(double.MaxValue, json["double_field"].Value<double>());
            Assert.Equal(Convert.ToDouble(decimal.MaxValue), json["decimal_field"].Value<double>());
            Assert.Equal(90.2m, json["geo_geopoint_field"]["lat"].Value<decimal>());
            Assert.Equal(180.1m, json["geo_geopoint_field"]["lon"].Value<decimal>());
            Assert.Equal("field1=value1|field2=value2", json["map_delimited_string_field"].Value<string>());
            Assert.Equal("value3", json["json_map_json_field"]["field3"].Value<string>());
            Assert.Equal("value4", json["json_map_json_field"]["field4"].Value<string>());
            Assert.Equal(IPAddress.Loopback.ToString(), json["ip_ipAddr"].Value<string>());
            
            var decompressedData = DecompressData(json["binary_payload"].Value<string>());
            var result = Utf8WithoutBom.GetString(decompressedData);
            Assert.Equal("payload_string", result);
        }

        private static byte[] DecompressData(string base64String)
        {
            var compressedData = Convert.FromBase64String(base64String);
            using (var compressedStream = new MemoryStream(compressedData))
            {
                using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress))
                {
                    using (var decompressedStream = new MemoryStream())
                    {
                        gzipStream.CopyTo(decompressedStream);
                        return decompressedStream.ToArray();
                    }
                }
            }
        }
    }
}