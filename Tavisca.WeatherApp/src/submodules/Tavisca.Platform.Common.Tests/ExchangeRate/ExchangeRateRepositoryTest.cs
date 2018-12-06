using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.ExchangeRate;
using Tavisca.Common.Plugins.ExchangeRate.Entities;
using Tavisca.Common.Plugins.RecyclableStreamPool;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.Plugins.Json;
using Xunit;
using Tavisca.Common.Plugins.ExchangeRate.Exceptions;
using Tavisca.Platform.Common.Models;
using Tavisca.Common.Plugins.ExchangeRate.Providers;
using System.Diagnostics;
using System.Linq;
using Tavisca.Common.Plugins.ExchangeRate.DAL;

namespace Tavisca.Platform.Common.Tests.ExchangeRate
{
    public class ExchangeRateServiceAdapterTest
    {
        private Mock<IHttpConnector> GetConnectorWithSuccessfulResponse() => CreateHttpConnector(CreateSuccessfulHttpResponse());

        private Mock<IHttpConnector> GetConnectorWithSuccessfulResponseWithSettings() => CreateHttpConnectorWithSettings(CreateSuccessfulHttpResponse());

        private Mock<IHttpConnector> CreateBadRequestHttpResponseConnector() => CreateHttpConnectorWithSettings(CreateBadRequestHttpResponse());

        private Mock<IHttpConnector> CreateInternalServerErrorHttpResponseConnector() => CreateHttpConnectorWithSettings(CreateInternalServerErrorHttpResponse());


        [Fact]
        public async Task GetCurrencyConversionRatesAsync_WithHttpDefaultsSet_ShouldReturnResults()
        {
            var configurationProvider = GetConfigurationProvider();

            SetHttpDefaults();

            var exchangeRateRepo = new ExchangeRateServiceAdapter(configurationProvider.Object);
            var exchangeRates = await exchangeRateRepo.GetCurrencyConversionRatesAsync();
            Assert.NotNull(exchangeRates);
            Assert.True(exchangeRates.Keys.Count > 0);
        }

        [Fact]
        public async Task GetCurrencyConversionRatesAsync_WithNoHttpDefaultsSet_ShouldReturnResults()
        {
            var configurationProvider = GetConfigurationProvider();

            var exchangeRateRepo = new ExchangeRateServiceAdapter(configurationProvider.Object, new JsonDotNetSerializer(new JsonSerializer()), new RecyclableStreamPool(), GetConnectorWithSuccessfulResponseWithSettings().Object);
            var exchangeRates = await exchangeRateRepo.GetCurrencyConversionRatesAsync();
            Assert.NotNull(exchangeRates);
            Assert.True(exchangeRates.Keys.Count > 0);
        }

        [Fact]
        public async Task GetCurrencyConversionRatesAsync_WithMissingConfiguration_ThrowsException()
        {
            var configurationProvider = new Mock<IConfigurationProvider>();
            configurationProvider.Setup(x => x.GetGlobalConfigurationAsync<ServiceSettings>(Constants.ConfigurationSections.ExternalServiceConfigurations, Constants.TenantSettings.ExchangeRateService))
                .ReturnsAsync(null);

            SetHttpDefaults();

            var exchangeRateRepo = new ExchangeRateServiceAdapter(configurationProvider.Object);
            await Assert.ThrowsAsync<ExchangeRateMissingConfiguration>(async () => await exchangeRateRepo.GetCurrencyConversionRatesAsync());
        }

        [Fact]
        public async Task GetCurrencyConversionRatesAsync_WithMissingUrlInConfiguration_ThrowsException()
        {
            var configurationProvider = new Mock<IConfigurationProvider>();
            configurationProvider.Setup(x => x.GetGlobalConfigurationAsync<ServiceSettings>(Constants.ConfigurationSections.ExternalServiceConfigurations, Constants.TenantSettings.ExchangeRateService))
                .ReturnsAsync(new ServiceSettings { Name = "", Endpoint = new Endpoint() });

            SetHttpDefaults();

            var exchangeRateRepo = new ExchangeRateServiceAdapter(configurationProvider.Object);
            await Assert.ThrowsAsync<ExchangeRateMissingConfiguration>(async () => await exchangeRateRepo.GetCurrencyConversionRatesAsync());
        }

        [Fact]
        public async Task GetCurrencyConversionRatesAsync_WithInvalidRequest_ThrowsException()
        {
            var configurationProvider = GetConfigurationProvider();

            var exchangeRateRepo = new ExchangeRateServiceAdapter(configurationProvider.Object, new JsonDotNetSerializer(new JsonSerializer()), new RecyclableStreamPool(), CreateBadRequestHttpResponseConnector().Object);
            await Assert.ThrowsAsync<BadRequestException>(async () => await exchangeRateRepo.GetCurrencyConversionRatesAsync());
        }

        [Fact]
        public async Task GetCurrencyConversionRatesAsync_WithInternalServerError_ThrowsException()
        {
            var configurationProvider = GetConfigurationProvider();

            var exchangeRateRepo = new ExchangeRateServiceAdapter(configurationProvider.Object, new JsonDotNetSerializer(new JsonSerializer()), new RecyclableStreamPool(), CreateInternalServerErrorHttpResponseConnector().Object);
            await Assert.ThrowsAsync<CommunicationException>(async () => await exchangeRateRepo.GetCurrencyConversionRatesAsync());
        }

        private static HttpResponse CreateSuccessfulHttpResponse()
        {
            var requestPayload = ByteHelper.ToByteArrayUsingJsonSerialization(
                new List<GetExchangeRatesResponse> {
                    { new GetExchangeRatesResponse
                        {
                            FromCurrency = "EUR",
                            SupportedConversions =
                                {
                                    new CurrencyConversionRate{ ToCurrency = "INR", ConversionFactor =74},
                                    new CurrencyConversionRate{ ToCurrency = "USD", ConversionFactor =0.90M}
                                }
                        }
                    },
                    { new GetExchangeRatesResponse
                        {
                            FromCurrency = "INR",
                            SupportedConversions =
                                {
                                    new CurrencyConversionRate{ ToCurrency = "USD", ConversionFactor =0.015M}
                                }
                        }
                    }
                });

            return new HttpResponse(System.Net.HttpStatusCode.OK, requestPayload());
        }

        private void SetHttpDefaults()
        {
            Http.ConfigureDefaults()
                .WithHttpConnector(GetConnectorWithSuccessfulResponse().Object)
                .WithSerializer(new JsonDotNetSerializer(new JsonSerializer()))
                .WithMemoryStreamPool(new RecyclableStreamPool());
        }

        private static Mock<IConfigurationProvider> GetConfigurationProvider()
        {
            var setting = new ServiceSettings() { Name = "mock", Endpoint = new Endpoint() { Url = new Uri("http://test:8080/exchangeRate/v1.0"), TimeOutInSeconds = 6000 } };
            var configurationProvider = new Mock<IConfigurationProvider>();
            configurationProvider.Setup(x => x.GetGlobalConfigurationAsync<ServiceSettings>(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(() => { return Task.FromResult(setting); });
            return configurationProvider;
        }

        private static Mock<IHttpConnector> CreateHttpConnector(HttpResponse response, bool throwFault = false)
        {
            var httpConnector = new Mock<IHttpConnector>();
            httpConnector.Setup(x => x.SendAsync(It.IsAny<HttpRequest>(), It.IsAny<CancellationToken>()))
                .Returns(() => { return Task.FromResult(ResponsdOrFault(response, throwFault)); });
            return httpConnector;
        }

        private static Mock<IHttpConnector> CreateHttpConnectorWithSettings(HttpResponse response, bool throwFault = false)
        {
            var httpConnector = new Mock<IHttpConnector>();
            var httpResponse = ResponsdOrFault(response, throwFault);

            httpConnector.Setup(x => x.SendAsync(It.IsAny<HttpRequest>(), It.IsAny<CancellationToken>()))
                .Returns(() => { return Task.FromResult(httpResponse); })
                .Callback<HttpRequest, CancellationToken>((r, c) => httpResponse.Settings = r.Settings);
            return httpConnector;
        }

        private static HttpResponse ResponsdOrFault(HttpResponse response, bool throwFault)
        {
            if (throwFault)
                throw new Exception("Test Message");
            return response;
        }

        private static HttpResponse CreateBadRequestHttpResponse()
        {
            var requestPayload = ByteHelper.ToByteArrayUsingJsonSerialization(
                new ErrorInfo("ABC", "InvalidRequest", System.Net.HttpStatusCode.BadRequest));

            return new HttpResponse(System.Net.HttpStatusCode.OK, requestPayload());
        }

        private static HttpResponse CreateInternalServerErrorHttpResponse()
        {
            var requestPayload = ByteHelper.ToByteArrayUsingJsonSerialization(
                new ErrorInfo("ABC", "InternalServerError", System.Net.HttpStatusCode.InternalServerError));

            return new HttpResponse(System.Net.HttpStatusCode.OK, requestPayload());
        }
    }
}
