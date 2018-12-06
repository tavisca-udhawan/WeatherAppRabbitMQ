using System;
using System.Collections.Generic;
using Tavisca.Common.Plugins.ExchangeRate.Entities;

namespace Tavisca.Common.Plugins.ExchangeRate
{
    [Serializable]

    public class GetExchangeRatesResponse
    {
        public string FromCurrency { get; set; }

        public List<CurrencyConversionRate> SupportedConversions { get; } = new List<CurrencyConversionRate>();

        public static Dictionary<CurrencyPair, decimal> ToExchangeRates( List<GetExchangeRatesResponse> response)
        {
            if (response == null)
                return null;

            var exchangeRates = new Dictionary<CurrencyPair, decimal>();
            foreach (var exchangeRate in response)
            {
                foreach (var currencyConversionRate in exchangeRate.SupportedConversions)
                {
                    exchangeRates.Add(new CurrencyPair(exchangeRate.FromCurrency, currencyConversionRate.ToCurrency), 
                        currencyConversionRate.ConversionFactor);
                }
            }

            return exchangeRates;
        }
    }
}
