using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.MemoryStreamPool;
using Tavisca.Platform.Common.Serialization;

namespace Tavisca.Platform.Common.WebApi
{
    public class JsonInputFormatter : InputFormatter
    {
        private readonly IMemoryStreamPool _memoryStreamPool;
        private readonly ISerializer _serializer;

        public JsonInputFormatter(ISerializer serializer, IMemoryStreamPool memoryStreamPool)
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));
            _serializer = serializer;
            _memoryStreamPool = memoryStreamPool;
        }

        private static readonly Encoding UTF8WithoutBom = new UTF8Encoding(false);

        public override bool CanRead(InputFormatterContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            if (context.ModelType == null)
                throw new ArgumentNullException(nameof(context.ModelType));
            return true;
        }


        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;

            string data = await new StreamReader(request.Body, UTF8WithoutBom).ReadToEndAsync();
            var bytes = Encoding.UTF8.GetBytes(data.ToCharArray());
            request.Body.Position = 0;
            try
            {
                using (var memoryStream = await _memoryStreamPool.GetMemoryStream(bytes))
                {
                    var output = _serializer.Deserialize(memoryStream, context.ModelType);
                    return InputFormatterResult.Success(output);
                }
            }
            catch (Exception ex)
            {
                using (var memoryStream = await _memoryStreamPool.GetMemoryStream(bytes))
                {
                    using (var reader = new StreamReader(memoryStream))
                    {
                        var requestData = await reader.ReadToEndAsync();
                        ex.Data["requestBody"] = requestData;
                        throw ex;
                    }
                }
            }
        }
    }
}
