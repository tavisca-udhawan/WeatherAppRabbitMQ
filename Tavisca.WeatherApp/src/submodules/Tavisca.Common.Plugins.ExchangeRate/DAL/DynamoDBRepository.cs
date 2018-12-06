using Amazon.DynamoDBv2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.ExchangeRate.Entities;
using Amazon.DynamoDBv2.DocumentModel;
using System.Collections.Specialized;
using Amazon;
using Tavisca.Platform.Common.Configurations;
using Newtonsoft.Json;

namespace Tavisca.Common.Plugins.ExchangeRate.DAL
{
    public class DynamoDBRepository : IExchangeRateRepository
    {

        private readonly IConfigurationProvider _configurationprovider;

        private NameValueCollection _settings = new NameValueCollection();

        public DynamoDBRepository(IConfigurationProvider configurationProvider)
        {
            this._configurationprovider = configurationProvider;
        }

        public async Task<Dictionary<CurrencyPair, decimal>> GetCurrencyConversionRatesAsync()
        {
            var exchangeRates = new Dictionary<CurrencyPair, decimal>();
            Exception exception = null;

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    exchangeRates = await GetFromDynamoDB();
                    if (exchangeRates != null)
                        break;

                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            if (exchangeRates.Count > 0)
                return exchangeRates;

            if (exception != null)
                throw exception;

            return null;
        }

        private async Task<Dictionary<CurrencyPair, decimal>> GetFromDynamoDB()
        {
            var exchangeRates = new Dictionary<CurrencyPair, decimal>();
            using (IAmazonDynamoDB client = await GetDynamoDBClientAsync())
            {
                var tableName = "CurrencyExchangeData";
                Table exchangeRateTable = Table.LoadTable(client, tableName);

                //get all records
                ScanFilter scanFilter = new ScanFilter();
                Search getAllItems = exchangeRateTable.Scan(scanFilter);

                List<Document> allItems = await getAllItems.GetRemainingAsync();

                exchangeRates = ToExchangeRates(allItems);

                return exchangeRates;
            }

        }

        private async Task<IAmazonDynamoDB> GetDynamoDBClientAsync()
        {
            try
            {
                // Throws Exception is validation fails
                await FetchConfigurations();

                ValidateConfigurations();

                var config = new AmazonDynamoDBConfig
                {
                    ServiceURL = _settings[Constants.DynamoDBServiceUrl]
                };

                var dbClient = new AmazonDynamoDBClient(_settings[Constants.AWSAccessKey], _settings[Constants.AWSSecretKey], config);

                return dbClient;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task FetchConfigurations()
        {
            try
            {
                var url = await _configurationprovider.GetGlobalConfigurationAsStringAsync(Constants.ApplicationName, Constants.SectionName, Constants.DynamoDBServiceUrl);
                var apiKey = await _configurationprovider.GetGlobalConfigurationAsStringAsync(Constants.ApplicationName, Constants.SectionName, Constants.AWSAccessKey);
                var secretKey = await _configurationprovider.GetGlobalConfigurationAsStringAsync(Constants.ApplicationName, Constants.SectionName, Constants.AWSSecretKey);

                _settings.Clear();
                _settings.Add(Constants.DynamoDBServiceUrl, url);
                _settings.Add(Constants.AWSAccessKey, apiKey);
                _settings.Add(Constants.AWSSecretKey, secretKey);
            }
            catch (Exception ex)
            {
                // Supressing exception as it is in background thread
                Platform.Common.ExceptionPolicy.HandleException(ex, "logonly");
            }
          
        }

        private void ValidateConfigurations()
        {
            if (_settings == null)
                throw new Exceptions.ExchangeRateMissingConfiguration();

            if (string.IsNullOrWhiteSpace(_settings[Constants.DynamoDBServiceUrl]))
                throw new Exceptions.ExchangeRateMissingConfiguration(Constants.DynamoDBServiceUrl);

            if (string.IsNullOrWhiteSpace(_settings[Constants.AWSSecretKey]))
                throw new Exceptions.ExchangeRateMissingConfiguration(Constants.AWSSecretKey);

            if (string.IsNullOrWhiteSpace(_settings[Constants.AWSAccessKey]))
                throw new Exceptions.ExchangeRateMissingConfiguration(Constants.AWSAccessKey);

        }

        private Dictionary<CurrencyPair, decimal> ToExchangeRates(List<Document> allItems)
        {
            var exchangeRates = new Dictionary<CurrencyPair, decimal>();

            foreach (Document item in allItems)
            {
                var sourceCurrency = string.Empty;
                var targetCurrencyJSON = string.Empty;

                foreach (string key in item.Keys)
                {
                    DynamoDBEntry dbEntry = item[key];
                    string val = dbEntry.ToString();

                    if (string.Equals(key, "source", StringComparison.InvariantCultureIgnoreCase))
                    {
                        sourceCurrency = val;
                    }
                    if (string.Equals(key, "target", StringComparison.InvariantCultureIgnoreCase))
                    {

                        targetCurrencyJSON = val;
                    }

                }

                var perCurrencyRates = ToModel(sourceCurrency, JsonConvert.DeserializeObject<List<KeyValue>>(targetCurrencyJSON));



                if (perCurrencyRates != null)
                    exchangeRates = Merge<CurrencyPair, decimal>(new List<Dictionary<CurrencyPair, decimal>>() { exchangeRates, perCurrencyRates });
            }

            return exchangeRates;
        }


        private Dictionary<TKey, TValue> Merge<TKey, TValue>(IEnumerable<Dictionary<TKey, TValue>> dictionaries)
        {

            var result = dictionaries.SelectMany(dict => dict)
                         .ToLookup(pair => pair.Key, pair => pair.Value)
                         .ToDictionary(group => group.Key, group => group.First());

            return result;

        }

        private Dictionary<CurrencyPair, decimal> ToModel(string sourceCurrency, List<KeyValue> list)
        {
            if (list == null)
                return null;

            var perCurrencyExchangeRates = new Dictionary<CurrencyPair, decimal>();

            list.ForEach(rate =>
            {
                decimal conversionFactor = 0;

                var isDecimal = decimal.TryParse(rate.Value, out conversionFactor);

                if (isDecimal)
                {
                    var currencyPair = new CurrencyPair(sourceCurrency, rate.Key);
                    perCurrencyExchangeRates.Add(currencyPair, conversionFactor);
                }

            });

            return perCurrencyExchangeRates;
        }
    }
}
