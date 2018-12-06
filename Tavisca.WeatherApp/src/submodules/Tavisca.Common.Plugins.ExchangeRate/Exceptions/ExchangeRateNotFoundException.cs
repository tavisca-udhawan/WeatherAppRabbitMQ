using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.ExchangeRate.Entities;

namespace Tavisca.Common.Plugins.ExchangeRate.Exceptions
{
    [Serializable]
    public class ExchangeRateNotFoundException : Exception, ISerializable
    {

        private const string StringFormat = "Exchange rate not found for a currency pair {0} and {1}";
        public ExchangeRateNotFoundException(CurrencyPair currencyPair) : base(string.Format(StringFormat, currencyPair.FromCurrency, currencyPair.ToCurrency))
        {

        }

        public ExchangeRateNotFoundException(string fromCurrency, string toCurrency) : base(string.Format(StringFormat, fromCurrency, toCurrency))
        {
        }

        public ExchangeRateNotFoundException()
        {
        }
        public ExchangeRateNotFoundException(string message) : base(message)
        {
        }
        public ExchangeRateNotFoundException(string message, Exception innerException) : base(message, innerException)
        {

        }

        // This constructor is needed for serialization.
        protected ExchangeRateNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}
