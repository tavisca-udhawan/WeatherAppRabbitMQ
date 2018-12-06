using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Serialization;

namespace Tavisca.Platform.Common
{
    [Serializable]
    public class HttpResponse
    {
        public HttpResponse(HttpStatusCode httpStatusCode, byte[] payload, HttpSettings settings = null)
        {
            Payload = payload;
            Status = httpStatusCode;
            Settings = HttpSettings.Resolve(settings, HttpSettings.Default);
            ApplySettings(Settings);
        }

        public byte[] Payload { get; private set; }
        public HttpStatusCode Status { get; }
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        public HttpSettings Settings { get; set; }
        public Dictionary<string, object> LogData { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        public ISerializer Serializer { get; set; }
        private object _cachedResponse = null;

        public bool IsSuccessful => (int)Status <= 300 && (int)Status >= 200;

        private void ApplySettings(HttpSettings settings)
        {
            if (settings == null) return;
            Serializer = settings.Serializer;
        }

        public async Task<T> GetResponseAsync<T>()
        {
            if (_cachedResponse is T)
                return (T)_cachedResponse;
            else
            {
                var serializer = Settings.Serializer;
                if (serializer == null)
                    throw new InvalidOperationException("Serializer is not initialized for HttpClient.");
                using (var stream = await GetResponseStream())
                {
                    var response = serializer.Deserialize<T>(stream);
                    // Cache the serialized response so that we do not have to deserialize it again.
                    _cachedResponse = response;
                    return response;
                }
            }
        }

        public async Task<ResponseOrFault<T, TEx>> GetResponseOrFaultAsync<T, TEx>()
        {
            // Check for cached state if available.
            if (_cachedResponse is T)
                return ResponseOrFault<T, TEx>.Successful((T)_cachedResponse);
            if (_cachedResponse is TEx)
                return ResponseOrFault<T, TEx>.Faulted((TEx)_cachedResponse);

            var serializer = Settings.Serializer;
            if (serializer == null)
                throw new InvalidOperationException("Serializer is not initialized for HttpClient.");

            using (var stream = await GetResponseStream())
            {
                if (this.IsSuccessful == true)
                {
                    try
                    {
                        var response = serializer.Deserialize<T>(stream);
                        // Cache the serialized response so that we do not have to deserialize it again.
                        _cachedResponse = response;
                        return ResponseOrFault<T, TEx>.Successful(response);
                    }
                    catch
                    {
                        stream.Position = 0;
                        // Cache the serialized response so that we do not have to deserialize it again.
                        var fault = serializer.Deserialize<TEx>(stream);
                        _cachedResponse = fault;
                        return ResponseOrFault<T, TEx>.Faulted(fault);
                    }
                }
                else
                {
                    try
                    {
                        var fault = serializer.Deserialize<TEx>(stream);
                        _cachedResponse = fault;
                        return ResponseOrFault<T, TEx>.Faulted(fault);
                    }
                    catch
                    {
                        stream.Position = 0;
                        var response = serializer.Deserialize<T>(stream);
                        _cachedResponse = response;
                        return ResponseOrFault<T, TEx>.Successful(response);
                    }
                }
            }
        }

        private async Task<MemoryStream> GetResponseStream()
        {
            var buffer = Settings.MemoryPool;
            if (buffer != null)
                return await buffer.GetMemoryStream(Payload);
            else
                return new MemoryStream(Payload);
        }
    }
}
