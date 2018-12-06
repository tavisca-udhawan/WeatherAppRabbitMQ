using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.ExchangeRate.Entities;
using Tavisca.Common.Plugins.ExchangeRate.Exceptions;
using Tavisca.Platform.Common;

namespace Tavisca.Common.Tests
{
    public class DummyExchangeRateProvider : IExchangeRateService
    {
        private readonly Dictionary<CurrencyPair, decimal> _staticExchangeRates;

        private readonly List<string> _validCurrencies = new List<string>()
        {
            "EUR","INR","AUD","USD"
        };

        public DummyExchangeRateProvider()
        {
            _staticExchangeRates = new Dictionary<CurrencyPair, decimal>()
            {
                { new CurrencyPair("EUR","INR"),74 },
                { new CurrencyPair("INR","USD"),0.015M },
                { new CurrencyPair("USD","EUR"),0.90M },
                { new CurrencyPair("USD","AUD"),0 }

            };
        }
#pragma warning disable 1998
        public decimal GetExchangeRate(string fromCurrency, string toCurrency)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(fromCurrency, toCurrency) == true)
                return 1;

            if (!_validCurrencies.Contains(fromCurrency))
            {
                throw new ArgumentException(fromCurrency);
            }
            else if (!_validCurrencies.Contains(toCurrency))
            {
                throw new ArgumentException(toCurrency);
            }
            decimal rate;
            if (_staticExchangeRates.TryGetValue(new CurrencyPair(fromCurrency, toCurrency), out rate))
                return rate;
            else if (_staticExchangeRates.TryGetValue(new CurrencyPair(toCurrency, fromCurrency), out rate))
                return 1M / rate;
            throw new ExchangeRateNotFoundException(fromCurrency, toCurrency);
        }
#pragma warning restore 1998

        public IEnumerable<string> GetSupportedCurrencies()
        {
            try
            {
                var currencyCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var exchangeRate in _staticExchangeRates.Keys)
                    currencyCodes.Add(exchangeRate.FromCurrency);

                return currencyCodes;
            }
            catch (Exception ex)
            {
                Platform.Common.ExceptionPolicy.HandleException(ex, "logonly");
            }

            return null;
        }
    }
}

