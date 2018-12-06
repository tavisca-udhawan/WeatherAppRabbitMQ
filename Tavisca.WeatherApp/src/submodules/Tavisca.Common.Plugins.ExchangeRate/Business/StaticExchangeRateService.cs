using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.ExchangeRate.Entities;
using Tavisca.Common.Plugins.ExchangeRate.Exceptions;
using Tavisca.Platform.Common;

namespace Tavisca.Common.Plugins.ExchangeRate.Providers
{
    public class StaticExchangeRateService : IExchangeRateService
    {

        private readonly Dictionary<CurrencyPair, decimal> _staticExchangeRates;

        public StaticExchangeRateService()
        {
            _staticExchangeRates = new Dictionary<CurrencyPair, decimal>()
            {
                { new CurrencyPair("EUR","INR"),74 },
                { new CurrencyPair("INR","USD"),0.015M },
                { new CurrencyPair("USD","EUR"),0.90M }

            };
        }

        public decimal GetExchangeRate(string fromCurrency, string toCurrency)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(fromCurrency, toCurrency) == true)
                return 1;
            decimal rate;
            if (_staticExchangeRates.TryGetValue(new CurrencyPair(fromCurrency, toCurrency), out rate))
                return rate;

            throw new ExchangeRateNotFoundException(fromCurrency, toCurrency);
        }

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


