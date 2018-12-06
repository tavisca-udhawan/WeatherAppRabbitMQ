using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.ExchangeRate;
using Tavisca.Common.Plugins.ExchangeRate.Entities;

namespace Tavisca.Common.Tests
{
    public class RefreshingDBRepository : IExchangeRateRepository
    {
        private static int index = 1;

        private static object locked = new object();
        public async Task<Dictionary<CurrencyPair, decimal>> GetCurrencyConversionRatesAsync()
        {
            return new Dictionary<CurrencyPair, decimal>()
            {
                { new CurrencyPair("EUR","INR"),GetNextValue()},
                { new CurrencyPair("INR","USD"),GetNextValue()},
                { new CurrencyPair("USD","EUR"),GetNextValue()},
                { new CurrencyPair("USD","AUD"),GetNextValue()}

            };
        }

        public int GetNextValue()
        {
            int indexer;
            lock (locked)
            {
                indexer = index++;
            }
            
            return indexer;
        }

      
    }
}
