using System;
using System.IO;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.MemoryStreamPool;
using Tavisca.Platform.Common.Models;
using Tavisca.Platform.Common.Serialization;

namespace Tavisca.Platform.Common.WebApi
{
    public class JsonFormatter : MediaTypeFormatter
    {
        public JsonFormatter(ISerializer serializer, IMemoryStreamPool memoryStreamPool)
        {
            _serializer = serializer;
            _memoryStreamPool = memoryStreamPool;
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
        }

        private readonly IMemoryStreamPool _memoryStreamPool;
        private readonly ISerializer _serializer;

        public override bool CanReadType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return true;
        }

        private static readonly Encoding UTF8WithoutBom = new UTF8Encoding(false);

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
            return ReadFromStreamAsync(type, readStream, content, formatterLogger, CancellationToken.None);
        }

        public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger, CancellationToken cancellationToken)
        {
            var bytes = await ReadStreamAsync(readStream);
            try
            {
                using (var memoryStream = await _memoryStreamPool.GetMemoryStream(bytes))
                {
                    return _serializer.Deserialize(memoryStream, type);
                }
            }
            catch (Exception ex) when (ex is BaseApplicationException == false)
            {
                using (var memoryStream = await _memoryStreamPool.GetMemoryStream(bytes))
                {
                    using (var reader = new StreamReader(memoryStream))
                    {
                        var request = await reader.ReadToEndAsync();
                        ex.Data["requestBody"] = request;
                        throw;
                    }
                }
            }
        }

        private async Task<byte[]> ReadStreamAsync(Stream readStream)
        {
            using (var memoryStream = await _memoryStreamPool.GetMemoryStream())
            {
                await readStream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
            return WriteToStreamAsync(type, value, writeStream, content, transportContext, CancellationToken.None);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext, CancellationToken cancellation)
        {
            _serializer.Serialize(writeStream, value, type);
            return Task.CompletedTask;
        }
    }
}