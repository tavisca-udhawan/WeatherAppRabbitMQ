using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.ExchangeRate.Entities;
using Tavisca.Common.Plugins.ExchangeRate.Exceptions;
using Tavisca.Platform.Common;

namespace Tavisca.Common.Plugins.ExchangeRate
{
    public class CurrencyConverter
    {

        private IExchangeRateService _exchangeRateProvider;
        public CurrencyConverter(IExchangeRateService exchangeRateProvider)
        {
            _exchangeRateProvider = exchangeRateProvider;
        }
        public decimal Convert(decimal amount, CurrencyPair currencyPair)
        {

            if (currencyPair.HaveSameCurrency())
                return amount;

            if (amount != 0)
            {
                var exchangeRate = GetExchangeRate(currencyPair);
                return amount * exchangeRate;
            }

            return amount;
        }

        public List<Entities.ExchangeRate> GetAllExchangeRates(List<CurrencyPair> currencyPairs)
        {
            if (currencyPairs == null || currencyPairs.Count == 0)
                return null;
            var exchangeRates = new List<ExchangeRate.Entities.ExchangeRate>();
            foreach (var currencyPair in currencyPairs)
            {
                if (currencyPair.HaveSameCurrency())
                    exchangeRates.Add(new Entities.ExchangeRate(currencyPair, 1));
                else
                {
                    var exchangeRate = GetSaferExchangeRate(currencyPair);
                    if (exchangeRate.HasValue)
                        exchangeRates.Add(new Entities.ExchangeRate(currencyPair, exchangeRate.Value));
                }
            }
            return exchangeRates;
        }

        public List<Entities.ExchangeRate> GetAllExchangeRates(List<string> fromCurrencies, string toCurrency)
        {

            var currencyPairs = fromCurrencies.Select(fromCurrency => new CurrencyPair(fromCurrency, toCurrency)).ToList();
            return GetAllExchangeRates(currencyPairs);

        }

        public List<Entities.ExchangeRate> GetAllExchangeRates(string fromCurrency, List<string> toCurrencies)
        {
            var currencyPairs = toCurrencies.Select(toCurrency => new CurrencyPair(fromCurrency, toCurrency)).ToList();
            return GetAllExchangeRates(currencyPairs);
        }

        public decimal GetExchangeRate(CurrencyPair currencyPair)
        {

            if (currencyPair.HaveSameCurrency())
                return 1;
            return _exchangeRateProvider.GetExchangeRate(currencyPair.FromCurrency, currencyPair.ToCurrency);

        }

        //This method is written to handle exception and return zero
        private decimal? GetSaferExchangeRate(CurrencyPair currencyPair)
        {
            try
            {
                return  GetExchangeRate(currencyPair);
            }
            catch (Exception)
            {
                //TODO: Exception logging here.
            }

            return null;
        }

    }
}
