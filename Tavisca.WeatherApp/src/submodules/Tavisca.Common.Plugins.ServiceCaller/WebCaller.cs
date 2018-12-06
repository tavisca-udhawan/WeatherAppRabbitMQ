using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tavisca.Common.Plugins.WebClient;
using WebClientRequestMessage = Tavisca.Common.Plugins.WebClient.WebClientRequestMessage;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    public class WebCaller
    {
        public static readonly CallerSettingsBuilder Configuration = new CallerSettingsBuilder();

        internal static readonly CallerSetting Settings = new CallerSetting();

        private Client _client;
        private Serializer _serializer;
        private Logger _logger;
        private readonly Type _errorPayloadType;
        private readonly ErrorPayloadTypeSelector _errorPayloadTypeSelector;
        private readonly ClientSetting _clientSetting;
        private readonly object _serializerSettings;

        static WebCaller()
        {
            Configuration.Apply();
        }

        public WebCaller()
        {
            SetDefaults();
        }

        public WebCaller(CallerSetting callerSetting)
        {
            _client = callerSetting.Client;
            _serializer = callerSetting.Serializer;
            _logger = callerSetting.Logger;
            new ClientSettingsValidator().Validate(callerSetting.ClientSetting);
            _clientSetting = callerSetting.ClientSetting;
            _serializerSettings = callerSetting.SerializerSetting;
            _errorPayloadType = callerSetting.ErrorPayloadType;
            _errorPayloadTypeSelector = callerSetting.ErrorPayloadTypeSelector;
            SetDefaults();
        }

        public WebCaller(Client client = null, Serializer serializer = null, Logger logger = null, ClientSetting clientSetting = null, object serializerSettings = null, Type errorPayloadType = null, ErrorPayloadTypeSelector errorPayloadTypeSelector = null)
        {
            _client = client;
            _serializer = serializer;
            _logger = logger;
            new ClientSettingsValidator().Validate(clientSetting);
            _clientSetting = clientSetting;
            _serializerSettings = serializerSettings;
            _errorPayloadType = errorPayloadType;
            _errorPayloadTypeSelector = errorPayloadTypeSelector;
            SetDefaults();
        }

        public async Task<IResponse<T2>> PostAsync<T1, T2>(WebPostRequest<T1> request)
        {
            byte[] postRequest = null;
            WebClientResponseMessage postResponse = null;
            WebResponse<T2> response = default(WebResponse<T2>);
            var startTimestamp = DateTime.UtcNow;
            Stopwatch watch = new Stopwatch();
            var serializeSettings = GetSerializerSettings(request.SerializerSettings);
            var clientSettings = GetClientSettings(request.ClientSetting);
            try
            {
                new RequestValidator().Validate(request);
                try
                {
                    postRequest = _serializer.Serialize(request.Request, serializeSettings);
                }
                catch (Exception exception)
                {
                    throw new SerializationException(request.Request.GetType(), exception);
                }
                try
                {
                    var webClientRequest = new WebClientRequestMessage()
                    {
                        Data = postRequest,
                        Url = request.EndPoint.Url
                    };
                    foreach (var headerName in clientSettings.Headers.AllKeys)
                    {
                        webClientRequest.RequestHeaders.Add(headerName, clientSettings.Headers[headerName]);
                    }
                    webClientRequest.ContentHeaders.Add("content-type", clientSettings.ContentType);
                    watch.Start();
                    postResponse = await _client.PostAsync(webClientRequest, clientSettings, new CancellationToken());
                    watch.Stop();
                }
                catch (Exception exception)
                {
                    if (watch.IsRunning)
                        watch.Stop();
                    throw new ClientCommunicationException(request.EndPoint, exception);
                }
                response = GetResponse<T2>(postResponse, serializeSettings);

            }
            finally
            {
                if (watch.IsRunning)
                    watch.Stop();
                var strRequest = string.Empty;
                if (postRequest != null)
                    strRequest = clientSettings.Encoding.GetString(postRequest);
                await Log(postResponse, request.Request, strRequest, response, watch.ElapsedMilliseconds, clientSettings, request.EndPoint.Url, startTimestamp);

            }
            return response;
        }
        public async Task<IResponse<T2>> PutAsync<T1, T2>(WebPostRequest<T1> request)
        {
            byte[] postRequest = null;
            WebClientResponseMessage postResponse = null;
            WebResponse<T2> response = default(WebResponse<T2>);
            var startTimestamp = DateTime.UtcNow;
            Stopwatch watch = new Stopwatch();
            var serializeSettings = GetSerializerSettings(request.SerializerSettings);
            var clientSettings = GetClientSettings(request.ClientSetting);
            try
            {
                new RequestValidator().Validate(request);
                try
                {
                    postRequest = _serializer.Serialize(request.Request, serializeSettings);
                }
                catch (Exception exception)
                {
                    throw new SerializationException(request.Request.GetType(), exception);
                }
                try
                {
                    var webClientRequest = new WebClientRequestMessage()
                    {
                        Data = postRequest,
                        Url = request.EndPoint.Url
                    };
                    foreach (var headerName in clientSettings.Headers.AllKeys)
                    {
                        webClientRequest.RequestHeaders.Add(headerName, clientSettings.Headers[headerName]);
                    }
                    webClientRequest.ContentHeaders.Add("content-type", clientSettings.ContentType);
                    watch.Start();
                    postResponse = await _client.PutAsync(webClientRequest, clientSettings, new CancellationToken());
                    watch.Stop();
                }
                catch (Exception exception)
                {
                    if (watch.IsRunning)
                        watch.Stop();
                    throw new ClientCommunicationException(request.EndPoint, exception);
                }
                response = GetResponse<T2>(postResponse, serializeSettings);

            }
            finally
            {
                if (watch.IsRunning)
                    watch.Stop();
                var strRequest = string.Empty;
                if (postRequest != null)
                    strRequest = clientSettings.Encoding.GetString(postRequest);
                await Log(postResponse, request.Request, strRequest, response, watch.ElapsedMilliseconds, clientSettings, request.EndPoint.Url, startTimestamp);

            }
            return response;
        }
        public async Task<IResponse<T>> GetAsync<T>(WebGetRequest request)
        {
            WebClientResponseMessage postResponse = null;
            WebResponse<T> response = default(WebResponse<T>);
            var serializeSettings = GetSerializerSettings(request.SerializerSettings);
            var startTimestamp = DateTime.UtcNow;
            var watch = new Stopwatch();
            var clientSettings = GetClientSettings(request.ClientSetting);
            try
            {
                new RequestValidator().Validate(request);
                try
                {
                    var webClientRequest = new WebClientRequestMessage()
                    {
                        Url = request.EndPoint.Url
                    };
                    foreach (var headerName in clientSettings.Headers.AllKeys)
                    {
                        webClientRequest.RequestHeaders.Add(headerName, clientSettings.Headers[headerName]);
                    }
                    webClientRequest.ContentHeaders.Add("content-type", clientSettings.ContentType);
                    watch.Start();
                    postResponse = await _client.GetAsync(webClientRequest, clientSettings, new CancellationToken());
                    watch.Stop();
                }
                catch (Exception exception)
                {
                    if (watch.IsRunning)
                        watch.Stop();
                    throw new ClientCommunicationException(request.EndPoint, exception);
                }
                response = GetResponse<T>(postResponse, serializeSettings);

            }
            finally
            {
                if (watch.IsRunning)
                    watch.Stop();
                var url = string.Empty;
                if (request.EndPoint != null)
                    url = request.EndPoint.Url;
                await Log(postResponse, null, url, response, watch.ElapsedMilliseconds, clientSettings, url, startTimestamp);

            }
            return response;
        }

        private async Task Log(WebClientResponseMessage postResponse, object request, string postRequest, object response, double timeTakenInMiliseconds, ClientSetting clientSetting, string url, DateTime startTimestamp)
        {
            try
            {
                if (_logger != null)
                {
                    var strResponse = string.Empty;
                    if (postResponse?.Data != null)
                        strResponse = clientSetting.Encoding.GetString(postResponse.Data);
                    await _logger.RaiseLogEvent(request, response, postRequest, strResponse, timeTakenInMiliseconds / 1000, postResponse, clientSetting.Headers, url, startTimestamp);
                }
            }
            catch (Exception exception)
            {
                throw new LoggingException("Exception occurred at the time of logging", exception);
            }
        }

        private WebResponse<T> GetResponse<T>(WebClientResponseMessage response, object serializerSettings)
        {
            try
            {
                if (response != null)
                {
                    var type = GetErrorPayloadType(response);
                    if (response.HttpStatusCode == HttpStatusCode.OK || type == typeof(T))
                        return new WebResponse<T>()
                        {
                            ReturnObject = _serializer.Deserialize<T>(response.Data, serializerSettings),
                            IsSuccess = true,
                            HttpStatusCode = response.HttpStatusCode

                        };
                    MethodInfo method = _serializer.GetType().GetMethod("Deserialize").MakeGenericMethod(type);
                    var errorPayload = method.Invoke(_serializer, new[] { response.Data, serializerSettings });
                    return new WebResponse<T>()
                    {
                        ErrorPayLoad = errorPayload,
                        IsSuccess = false,
                        HttpStatusCode = response.HttpStatusCode
                    };
                }
            }

            catch (Exception exception)
            {
                throw new SerializationException("Exception occurred at the time of desrialization", exception);
            }
            return new WebResponse<T>() { IsSuccess = false };

        }

        private ClientSetting GetClientSettings(ClientSetting inputClientSetting)
        {
            if (inputClientSetting != null)
                return inputClientSetting;
            else if (_clientSetting != null)
                return _clientSetting;
            else
                return Settings.ClientSetting;
        }

        private object GetSerializerSettings(JsonSerializerSettings inputSerializerSettings)
        {
            if (inputSerializerSettings != null)
                return inputSerializerSettings;
            else if (_serializerSettings != null)
                return _serializerSettings;
            else
                return Settings.SerializerSetting;
        }

        private Type GetErrorPayloadType(WebClientResponseMessage webClientResponseMessage)
        {
            Type selectedPayloadType = DefaultCallerSettings.DefaultErrorPayloadType;
            if (_errorPayloadType != null)
                selectedPayloadType = _errorPayloadType;
            else if (Settings.ErrorPayloadType != null)
                selectedPayloadType = Settings.ErrorPayloadType;
            if (_errorPayloadTypeSelector != null)
                return _errorPayloadTypeSelector.GetReturnObjectType(webClientResponseMessage, selectedPayloadType);
            return selectedPayloadType;
        }


        private void SetDefaults()
        {

            if (_client == null)
                _client = Settings.Client;

            if (_logger == null)
                _logger = Settings.Logger;

            if (_serializer == null)
                _serializer = Settings.Serializer;

        }


    }
}
