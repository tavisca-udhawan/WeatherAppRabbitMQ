using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common
{
    public interface IExchangeRateService
    {
        decimal GetExchangeRate(string fromCurrency, string toCurrency);

        IEnumerable<string> GetSupportedCurrencies();
    }
}
