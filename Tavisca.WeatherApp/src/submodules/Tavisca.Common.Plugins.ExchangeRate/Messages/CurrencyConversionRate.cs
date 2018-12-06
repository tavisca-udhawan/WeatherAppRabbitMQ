using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.ExchangeRate
{
    [Serializable]
    public class CurrencyConversionRate
    {
        public string ToCurrency { get; set; }
        public decimal ConversionFactor { get; set; }
    }
}
