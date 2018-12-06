using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.MemoryStreamPool;
using Tavisca.Platform.Common.Models;

namespace Tavisca.Platform.Common
{
    public class WebRequestConnector : IHttpConnector
    {
        private readonly IMemoryStreamPool _memoryStreamPool;
        private static byte[] EmptyPayload = new byte[0];

        public WebRequestConnector(IMemoryStreamPool memoryStreamPool = null)
        {
            _memoryStreamPool = memoryStreamPool ?? HttpSettings.Default.MemoryPool;
        }

        private HttpWebRequest CreateBasicWebRequest(HttpRequest request)
        {
            var webRequest = WebRequest.Create(request.Uri) as HttpWebRequest;
            if (webRequest == null)
                return null;

            webRequest.Headers = new WebHeaderCollection();
            webRequest.Method = request.Method;
            webRequest.ContentType = request.ContentType;
            if (request.IsProxyDisabled == true)
                webRequest.Proxy = null;
#if !NET_STANDARD
            webRequest.ProtocolVersion = new Version(request.ProtocolVersion);
            webRequest.ServicePoint.Expect100Continue = false;
            webRequest.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            webRequest.Timeout = (int)request?.TimeOut.Value.TotalMilliseconds;
#endif
            foreach (var header in request.Headers)
                webRequest.Headers[header.Key] = header.Value;
            return webRequest;
        }

        public async Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var webRequest = CreateBasicWebRequest(request);

            if (request.SendRequestBody == true)
            {
                Stream compressionStream = null;
                request.Payload = request.Payload ?? EmptyPayload;
                using (var requestStream = await webRequest.GetRequestStreamAsync())
                {
                    if (request.IsCompressionEnabled == false)
                        await requestStream.WriteAsync(request.Payload, 0, request.Payload.Length, cancellationToken);
                    else
                    {
                        webRequest.Headers[HeaderNames.ContentEncoding] = request.CompressionType.ToString().ToLower();
                        if (request.CompressionType == CompressionType.Gzip)
                            compressionStream = new GZipStream(requestStream, CompressionMode.Compress, false);
                        else
                            compressionStream = new DeflateStream(requestStream, CompressionMode.Compress, false);
                        using (compressionStream)
                        {
                            await compressionStream.WriteAsync(request.Payload, 0, request.Payload.Length, cancellationToken);
                        }
                    }
                }
            }

            var timeOut = request.TimeOut.Value;
            var cts = new CancellationTokenSource(timeOut);
            var token = cts.Token;
            var responseTask = GetHttpResponseAsync(request, webRequest, token);
            await Task.WhenAny(responseTask, Task.Delay(timeOut, token));

            if (!responseTask.IsCompleted)
                throw new CommunicationException("32", "A timeout exception occured", HttpStatusCode.GatewayTimeout);

            return await responseTask;
        }

        private async Task<HttpResponse> GetHttpResponseAsync(HttpRequest request, HttpWebRequest webRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            HttpWebResponse webResponse;
            HttpResponse response;

            try
            {
                webResponse = await webRequest.GetResponseAsync() as HttpWebResponse;
            }
            catch (WebException webException)
            {
                webResponse = webException.Response as HttpWebResponse;
                if (webResponse == null)
                {
                    throw webException;
                }
            }

            using (var responseStream = webResponse.GetResponseStream())
            {
                Stream streamToCopy = responseStream;
                using (var memoryStream = await CreateStreamAsync())
                {

#if NET_STANDARD
                    var contentEncodingKey = webResponse.Headers.AllKeys.ToList().FirstOrDefault(h => h.ToLower().Equals("content-encoding"));
                    if (!string.IsNullOrEmpty(contentEncodingKey))
                    {
                        Stream decompressStream = null;
                        if (webResponse.Headers[contentEncodingKey].Contains("gzip"))
                            decompressStream = new GZipStream(responseStream, CompressionMode.Decompress, false);
                        else if (webResponse.Headers[contentEncodingKey].Contains("deflate"))
                            decompressStream = new DeflateStream(responseStream, CompressionMode.Decompress, false);
                        using (decompressStream)
                        {
                            if (decompressStream != null)
                                await decompressStream.CopyToAsync(memoryStream);
                            else
                                await responseStream.CopyToAsync(memoryStream);
                        }
                    }
                    else
                        await responseStream.CopyToAsync(memoryStream);
#else
                    await responseStream.CopyToAsync(memoryStream);
#endif

                    var payload = memoryStream.ToArray();
                    response = new HttpResponse(webResponse.StatusCode, payload, request.Settings);
                    WriteHeaders(webResponse?.Headers, response);
                }
            }

            webResponse.Close();
            webResponse.Dispose();

            return response;
        }

        private void SetTimeOut(CancellationTokenSource cancellationTokenSource, TimeSpan? timeOut)
        {
            if (timeOut != null)
            {
                cancellationTokenSource.CancelAfter(timeOut.Value);
            }
        }

        private async Task<MemoryStream> CreateStreamAsync()
        {
            if (_memoryStreamPool == null)
                return new MemoryStream();
            else return await _memoryStreamPool.GetMemoryStream();
        }

        private void WriteHeaders(WebHeaderCollection headers, HttpResponse response)
        {
            if (headers != null)
            {
                foreach (var header in headers.AllKeys)
                    response.Headers[header] = headers[header];
            }
        }
    }
}
