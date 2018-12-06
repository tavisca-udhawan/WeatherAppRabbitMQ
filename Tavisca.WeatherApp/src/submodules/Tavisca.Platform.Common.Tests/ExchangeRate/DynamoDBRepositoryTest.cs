using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Tavisca.Common.Plugins.ExchangeRate.Entities;
using Tavisca.Common.Plugins.ExchangeRate;
using Tavisca.Common.Plugins.ExchangeRate.DAL;
using Tavisca.Common.Plugins.ExchangeRate.Exceptions;

namespace Tavisca.Common.Tests
{
    [Ignore]
    [TestClass]
    public class DynamoDBRepositoryTest
    {

        const string DynamoDBServiceUrl = "https://dynamodb.us-east-1.amazonaws.com";

        const string AWSSecretKey = "put your key";

        const string AWSAPIKey = "put your key";

        DummyConfigurationStore GetConfigStore(string dynamoDBServiceUrl, string awsSecretKey, string awsAPIKey)
        {

            var configStore = new DummyConfigurationStore();
            configStore.Configuration = new System.Collections.Specialized.NameValueCollection();
            configStore.Configuration.Add(Constants.DynamoDBServiceUrl, dynamoDBServiceUrl);
            configStore.Configuration.Add(Constants.AWSSecretKey, awsSecretKey);
            configStore.Configuration.Add(Constants.AWSAccessKey, awsAPIKey);

            return configStore;
        }

        private IExchangeRateRepository GetExchangeRateRepository()
        {
            return new DynamoDBRepository(GetConfigStore(DynamoDBServiceUrl, AWSSecretKey, AWSAPIKey));
        }

        private IExchangeRateRepository GetExchangeRateRepositoryWithMissingConfigSection()
        {
            return new DynamoDBRepository(new DummyConfigurationStore());
        }

        private IExchangeRateRepository GetExchangeRateRepositoryWithMissingAwsAPIKey()
        {
            return new DynamoDBRepository(GetConfigStore(DynamoDBServiceUrl, AWSSecretKey, string.Empty));
        }

        private IExchangeRateRepository GetExchangeRateRepositoryWithMissingAwsSecretKey()
        {
            return new DynamoDBRepository(GetConfigStore(DynamoDBServiceUrl, string.Empty, AWSAPIKey));
        }

        private IExchangeRateRepository GetExchangeRateRepositoryWithMissingDBServiceURL()
        {
            return new DynamoDBRepository(GetConfigStore(string.Empty, AWSSecretKey, AWSAPIKey));
        }

        private IExchangeRateRepository GetExchangeRateRepositoryWithInvalidDBServiceURL()
        {
            return new DynamoDBRepository(GetConfigStore("https://dynamodb.us-west-1.amazonaws.com", AWSSecretKey, AWSAPIKey));
        }
        private IExchangeRateRepository GetExchangeRateRepositoryWithInvalidAWScredentials()
        {
            return new DynamoDBRepository(GetConfigStore(DynamoDBServiceUrl, "nDzgYQTds5xl16BPoiuGqCuFTh5+2M/1ZBJJqNN6", "AKIAIWO6OQDP24K6VO7L"));
        }

        [TestMethod]
        public async Task DyanmoDBReadDataFromSource()
        {
            var currencyConversionRepo = GetExchangeRateRepository();
            var exchangeRates = await currencyConversionRepo.GetCurrencyConversionRatesAsync();
            Assert.IsNotNull(exchangeRates);
            Assert.IsTrue(exchangeRates.Keys.Count > 0);

        }

        [TestMethod]
        public async Task DynmoDBMissingConfigurationThrowException()
        {
            var currencyConversionRepo = GetExchangeRateRepositoryWithMissingConfigSection();

            await Xunit.Assert.ThrowsAsync<ExchangeRateMissingConfiguration>(() => currencyConversionRepo.GetCurrencyConversionRatesAsync());

        }


        [TestMethod]
        public async Task DynmoDBMissingAWSAPIKeyThrowException()
        {
            var currencyConversionRepo = GetExchangeRateRepositoryWithMissingAwsAPIKey();
            await Xunit.Assert.ThrowsAsync<ExchangeRateMissingConfiguration>(() => currencyConversionRepo.GetCurrencyConversionRatesAsync());

        }


        [TestMethod]
        public async Task DynmoDBMissingAWSSecretKeyThrowException()
        {
            var currencyConversionRepo = GetExchangeRateRepositoryWithMissingAwsSecretKey();
            await Xunit.Assert.ThrowsAsync<ExchangeRateMissingConfiguration>(() => currencyConversionRepo.GetCurrencyConversionRatesAsync());

        }

        [TestMethod]
        public async Task DynmoDBMissingServiceUrlThrowException()
        {
            var currencyConversionRepo = GetExchangeRateRepositoryWithMissingDBServiceURL();
            await Xunit.Assert.ThrowsAsync<ExchangeRateMissingConfiguration>(() => currencyConversionRepo.GetCurrencyConversionRatesAsync());
        }

        [TestMethod]
        public async Task DynmoDBInvalidServiceUrlThrowException()
        {
            var currencyConversionRepo = GetExchangeRateRepositoryWithInvalidDBServiceURL();
            await Xunit.Assert.ThrowsAsync<Amazon.DynamoDBv2.Model.ResourceNotFoundException>(() => currencyConversionRepo.GetCurrencyConversionRatesAsync());
        }

        [TestMethod]
        public async Task DynmoDBInvalidAWScredentialsThrowException()
        {
            var currencyConversionRepo = GetExchangeRateRepositoryWithInvalidAWScredentials();
            await Xunit.Assert.ThrowsAsync<AmazonDynamoDBException>(() => currencyConversionRepo.GetCurrencyConversionRatesAsync());
        }


    }
}
