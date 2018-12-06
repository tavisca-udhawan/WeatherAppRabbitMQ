using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.ExchangeRate.Entities;
using Tavisca.Platform.Common;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.MemoryStreamPool;
using Tavisca.Platform.Common.Models;
using Tavisca.Platform.Common.Serialization;

namespace Tavisca.Common.Plugins.ExchangeRate
{
    public class ExchangeRateServiceAdapter : IExchangeRateRepository
    {
        private readonly IConfigurationProvider _configurationProvider;
        private readonly ISerializer _serializer;
        private readonly IMemoryStreamPool _memoryPool;
        private readonly IHttpConnector _httpConnector;
        private HttpSettings _httpSettings;

        //Assumptions ISerializer, IMemoryStreamPool(optional), IHttpConnector(optional) should be initialized using Http.ConfigureDefaults()
        public ExchangeRateServiceAdapter(IConfigurationProvider configurationProvider)
        {
            _configurationProvider = configurationProvider;
        }

        public ExchangeRateServiceAdapter(IConfigurationProvider configurationProvider, ISerializer serializer, IMemoryStreamPool memoryPool = null, IHttpConnector httpConnector = null)
        {
            _configurationProvider = configurationProvider;
            _httpSettings = new HttpSettings();

            if (serializer != null)
                _httpSettings.WithSerializer(serializer);

            if (memoryPool != null)
                _httpSettings.WithMemoryStreamPool(memoryPool);

            if (httpConnector != null)
            {
                Func<IHttpConnector> func = () => httpConnector;
                _httpSettings.WithConnector(func);
            }
            var ip = GetSystemLocalIp();
            _httpSettings.Headers.Add(Constants.OskiUserIp, ip.Equals(string.Empty) ? Constants.UserIp : ip);
        }

        public async Task<Dictionary<CurrencyPair, decimal>> GetCurrencyConversionRatesAsync()
        {
            //build request
            var request = await GetRequest(Constants.ExchangeRateService.GetAllRates);

            //call service using webcaller
            var httpResponse = await request.SendAsync();

            //parse and return response
            return await ParseResponse(httpResponse);
        }

        private async Task<HttpRequest> GetRequest(string apiName)
        {
            var settings = await GetExchangeRateServiceSettings();

            var uri = new Uri(settings.Endpoint.Url + apiName);
            settings.Endpoint.Url = uri;

            if (_httpSettings != null)
            {             
                AddCustomHeaders(settings.Endpoint.CustomHeaders);

                _httpSettings
                    .WithTimeOut(new TimeSpan(0, 0, 0, settings.Endpoint.TimeOutInSeconds, 0));
            }

            return Http.NewPostRequest(settings.Endpoint.Url, _httpSettings);
        }

        private void AddCustomHeaders( Dictionary<string, string> customHeaders)
        {
            foreach (var key in customHeaders.Keys)
                _httpSettings.Headers[key] = customHeaders[key];
        }

        private string GetSystemLocalIp()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return string.Empty;
        }

        private async Task<ServiceSettings> GetExchangeRateServiceSettings()
        {
            var settings = await _configurationProvider.GetGlobalConfigurationAsync<ServiceSettings>(Constants.ConfigurationSections.ExternalServiceConfigurations, Constants.TenantSettings.ExchangeRateService);

            if (settings == null)
                throw new Exceptions.ExchangeRateMissingConfiguration();

            if (settings.Endpoint.Url == null)
                throw new Exceptions.ExchangeRateMissingConfiguration(nameof(settings.Endpoint.Url));

            return settings;
        }

        private async Task<Dictionary<CurrencyPair, decimal>> ParseResponse(HttpResponse httpResponse)
        {
            var response = await httpResponse?.GetResponseOrFaultAsync<List<GetExchangeRatesResponse>, ErrorInfo>();

            if (response != null && response.Fault != null && (int)response.Fault.HttpStatusCode >= 400 && (int)response.Fault.HttpStatusCode < 500)
                throw new BadRequestException(ConvertErrorInfo(response.Fault));

            if (response != null && response.IsFaulted)
                throw Errors.ServerSide.ServiceCommunication(nameof(Constants.TenantSettings.ExchangeRateService));

            return GetExchangeRatesResponse.ToExchangeRates(response.Response);
        }

        private ErrorInfo ConvertErrorInfo(ErrorInfo errorInfo)
        {
            var result = new ErrorInfo(errorInfo.Code, errorInfo.Message, errorInfo.HttpStatusCode);
            if (errorInfo.Info != null)
            {
                foreach (var info in errorInfo.Info)
                {
                    result.Info.Add(new Info(info.Code, info.Message));
                }
            }
            return result;
        }
    }
}
