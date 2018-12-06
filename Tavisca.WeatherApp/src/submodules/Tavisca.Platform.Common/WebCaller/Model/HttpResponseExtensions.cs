using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Serialization;

namespace Tavisca.Platform.Common.WebCaller.Model
{
    public static class HttpResponseExtensions
    {
        public static HttpResponse WithLogData(this HttpResponse res, string name, string value)
        {
            res.LogData[name] = value;
            return res;
        }

        public static HttpRequest WithLogData(this HttpRequest res, string name, IPAddress value)
        {
            res.LogData[name] = value;
            return res;
        }

        public static HttpRequest WithLogData(this HttpRequest res, string name, Payload value)
        {
            res.LogData[name] = value;
            return res;
        }

        public static HttpRequest WithLogData(this HttpRequest res, string name, GeoPoint value)
        {
            res.LogData[name] = value;
            return res;
        }

        public static HttpRequest WithLogData(this HttpRequest res, string name, DateTime value)
        {
            res.LogData[name] = value;
            return res;
        }

        public static HttpRequest WithLogData(this HttpRequest res, string name, long value)
        {
            res.LogData[name] = value;
            return res;
        }

        public static HttpRequest WithLogData(this HttpRequest res, string name, int value)
        {
            res.LogData[name] = value;
            return res;
        }

        public static HttpRequest WithLogData(this HttpRequest res, string name, ulong value)
        {
            res.LogData[name] = value;
            return res;
        }

        public static HttpRequest WithLogData(this HttpRequest res, string name, uint value)
        {
            res.LogData[name] = value;
            return res;
        }

        public static HttpRequest WithLogData(this HttpRequest res, string name, float value)
        {
            res.LogData[name] = value;
            return res;
        }

        public static HttpRequest WithLogData(this HttpRequest res, string name, double value)
        {
            res.LogData[name] = value;
            return res;
        }

        public static HttpRequest WithLogData(this HttpRequest res, string name, decimal value)
        {
            res.LogData[name] = value;
            return res;
        }

        public static HttpRequest WithLogData(this HttpRequest res, string name, bool value)
        {
            res.LogData[name] = value;
            return res;
        }

        public static HttpRequest WithLogData(this HttpRequest res, string name, byte[] value, Encoding encoding = null)
        {
            res.LogData[name] = new Payload(value, encoding);
            return res;
        }

        public static HttpRequest WithLogData(this HttpRequest res, string name, IDictionary<string, string> map, MapFormat format = MapFormat.DelimitedString)
        {
            res.LogData[name] = new Map(map, format);
            return res;
        }

        public static HttpResponse WithSerializer(this HttpResponse res, ISerializer serializer)
        {
            res.Serializer = serializer;
            return res;
        }
    }
}
