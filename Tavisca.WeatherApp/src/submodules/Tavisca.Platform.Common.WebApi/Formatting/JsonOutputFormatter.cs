using Microsoft.AspNetCore.Mvc.Formatters;
using System;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Serialization;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Tavisca.Platform.Common.WebApi
{
    public class JsonOutputFormatter : OutputFormatter
    {
        private readonly ISerializer _serializer;
        private readonly string _contentType;

        public JsonOutputFormatter(ISerializer serializer)
        {
            SupportedMediaTypes.Add(new Microsoft.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
            _serializer = serializer;
            _contentType = "application/json";
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            /* ObjectType is set to null when controller returns 'null'
            * if (context.ObjectType == null)
               throw new ArgumentNullException(nameof(context.ObjectType));
               */
            return true;
        }

        public override async Task WriteResponseBodyAsync(OutputFormatterWriteContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var response = context.HttpContext.Response;
            response.ContentType = _contentType;

            string data = null;
            using (var stream = new MemoryStream())
            {
                _serializer.Serialize(stream, context.Object, context.ObjectType);
                data = Encoding.UTF8.GetString(stream.ToArray());

            }
            await response.WriteAsync(data.ToString());
        }
    }
}
