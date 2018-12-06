using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.ExchangeRate;
using Tavisca.Common.Plugins.ExchangeRate.Entities;

namespace Tavisca.Common.Tests
{
    public class StaticDBRepository : IExchangeRateRepository
    {
     internal   Dictionary<CurrencyPair, decimal> StaticExchangeRates = new Dictionary<CurrencyPair, decimal>()
            {
                { new CurrencyPair("EUR","INR"),74 },
                { new CurrencyPair("INR","USD"),0.015M },
                { new CurrencyPair("USD","EUR"),0.90M },
                { new CurrencyPair("USD","AUD"),0 }

            };

        public  async Task<Dictionary<CurrencyPair, decimal>> GetCurrencyConversionRatesAsync()
        {
           return StaticExchangeRates;
        }
    }
}
