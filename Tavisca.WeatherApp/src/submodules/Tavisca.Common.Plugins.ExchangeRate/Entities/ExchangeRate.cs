using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.ExchangeRate.Entities
{
    public class ExchangeRate
    {

        public ExchangeRate(CurrencyPair currencyPair, decimal exchangeRateValue)
        {
            CurrencyPair = currencyPair;
            Rate = exchangeRateValue;
        }

        public ExchangeRate(string fromCurrency, string toCurrency, decimal exchangeRateValue)
        {
            CurrencyPair = new CurrencyPair(fromCurrency, toCurrency);
            Rate = exchangeRateValue;
        }
        public CurrencyPair CurrencyPair { get;  private set; }
        public decimal Rate { get; private set; }
    }
}
