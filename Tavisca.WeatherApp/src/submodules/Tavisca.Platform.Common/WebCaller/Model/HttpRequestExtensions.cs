using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.MemoryStreamPool;

namespace Tavisca.Platform.Common
{

    public static class HttpRequestExtensions
    {
        public static HttpRequest WithHeader(this HttpRequest req, string header, string value)
        {
            req.Headers[header] = value;
            return req;
        }

        public static HttpRequest WithFaultPolicy(this HttpRequest req, IFaultPolicy policy)
        {
            req.FaultPolicy = policy;
            return req;
        }

        public static HttpRequest WithFaultPolicy(this HttpRequest req, Func<HttpRequest, HttpResponse, Task<bool>> func)
        {
            req.FaultPolicy = new DelegatedFaultPolicy(func);
            return req;
        }

        public static HttpRequest WithContentType(this HttpRequest req, string contentType)
        {
            req.ContentType = contentType;
            return req;
        }

        public static HttpRequest WithMaxResponseBufferSize(this HttpRequest req, int maxBufferSize)
        {
            req.MaxResponseBufferSize = maxBufferSize;
            return req;
        }

        public static HttpRequest WithTimeout(this HttpRequest req, TimeSpan timeout)
        {
            req.TimeOut = timeout;
            return req;
        }

        public static HttpRequest WithProtocolVersion(this HttpRequest req, string version)
        {
            req.ProtocolVersion = version;
            return req;
        }

        public static HttpRequest WithGZipCompression(this HttpRequest req)
        {
            req.CompressionType = CompressionType.Gzip;
            return req;
        }

        public static HttpRequest WithDeflateCompression(this HttpRequest req)
        {
            req.CompressionType = CompressionType.Deflate;
            return req;
        }

        public static HttpRequest WithBody(this HttpRequest req, byte[] payload)
        {
            req.Content = new ByteContent(payload);
            return req;
        }

        public static HttpRequest WithBody<T>(this HttpRequest req, T obj)
        {
            req.Content = new ObjectContent<T>(obj);
            return req;
        }

        public static HttpRequest WithHttpFilter(this HttpRequest req, HttpFilter filter)
        {
            req.HttpFilters.Add(filter);
            return req;
        }

        public static HttpRequest WithLogData(this HttpRequest req, string name, string value)
        {
            req.LogData[name] = value;
            return req;
        }

        public static HttpRequest WithLogData(this HttpRequest req, string name, IPAddress value)
        {
            req.LogData[name] = value;
            return req;
        }

        public static HttpRequest WithLogData(this HttpRequest req, string name, Payload value)
        {
            req.LogData[name] = value;
            return req;
        }

        public static HttpRequest WithLogData(this HttpRequest req, string name, GeoPoint value)
        {
            req.LogData[name] = value;
            return req;
        }

        public static HttpRequest WithLogData(this HttpRequest req, string name, DateTime value)
        {
            req.LogData[name] = value;
            return req;
        }

        public static HttpRequest WithLogData(this HttpRequest req, string name, long value)
        {
            req.LogData[name] = value;
            return req;
        }

        public static HttpRequest WithLogData(this HttpRequest req, string name, int value)
        {
            req.LogData[name] = value;
            return req;
        }

        public static HttpRequest WithLogData(this HttpRequest req, string name, ulong value)
        {
            req.LogData[name] = value;
            return req;
        }

        public static HttpRequest WithLogData(this HttpRequest req, string name, uint value)
        {
            req.LogData[name] = value;
            return req;
        }

        public static HttpRequest WithLogData(this HttpRequest req, string name, float value)
        {
            req.LogData[name] = value;
            return req;
        }

        public static HttpRequest WithLogData(this HttpRequest req, string name, double value)
        {
            req.LogData[name] = value;
            return req;
        }

        public static HttpRequest WithLogData(this HttpRequest req, string name, decimal value)
        {
            req.LogData[name] = value;
            return req;
        }

        public static HttpRequest WithLogData(this HttpRequest req, string name, bool value)
        {
            req.LogData[name] = value;
            return req;
        }

        public static HttpRequest WithLogData(this HttpRequest req, string name, byte[] value, Encoding encoding = null)
        {
            req.LogData[name] = new Payload(value, encoding);
            return req;
        }

        public static HttpRequest WithLogData(this HttpRequest req, string name, IDictionary<string, string> map, MapFormat format = MapFormat.DelimitedString)
        {
            req.LogData[name] = new Map(map, format);
            return req;
        }
    }
}
