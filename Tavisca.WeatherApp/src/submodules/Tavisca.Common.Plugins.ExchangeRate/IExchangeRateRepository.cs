using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.ExchangeRate.Entities;

namespace Tavisca.Common.Plugins.ExchangeRate
{
    public interface IExchangeRateRepository
    {
        Task<Dictionary<CurrencyPair, decimal>> GetCurrencyConversionRatesAsync();
    }
}
