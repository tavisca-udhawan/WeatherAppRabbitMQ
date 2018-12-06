using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.ExchangeRate.Entities
{
    public struct CurrencyPair : IEquatable<CurrencyPair>
    {

        public CurrencyPair(string fromCurrency, string toCurrency)
        {
            Validate(fromCurrency, toCurrency);
            FromCurrency = fromCurrency.ToUpper();
            ToCurrency = toCurrency.ToUpper();

        }


        public string FromCurrency { get; private set; }

        public string ToCurrency { get; private set; }


        public override bool Equals(object otherObject)
        {
            if (!(otherObject is CurrencyPair))
                return false;

            return this.Equals((CurrencyPair)otherObject);
        }

        public override int GetHashCode()
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(FromCurrency ?? string.Empty) ^ StringComparer.OrdinalIgnoreCase.GetHashCode(ToCurrency ?? string.Empty);
        }

        public CurrencyPair Reverse()
        {
            return new CurrencyPair(ToCurrency, FromCurrency);
        }

        private static bool Validate(string fromCurrency, string toCurrency)
        {
            if (fromCurrency == null && toCurrency == null)
                throw new System.ArgumentNullException("Currency pair should not be null or should contain both from currency and to currency");

            if (fromCurrency != null)
            {
                if (toCurrency != null)
                    return true;
                throw new System.ArgumentNullException(nameof(toCurrency));

            }
            throw new System.ArgumentNullException(nameof(fromCurrency));
        }

        public bool HaveSameCurrency()
        {
            return StringComparer.OrdinalIgnoreCase.Equals(this.FromCurrency, this.ToCurrency);
        }

        public bool Equals(CurrencyPair otherCurrencyPair)
        {
            if (this.FromCurrency == otherCurrencyPair.FromCurrency && this.ToCurrency == otherCurrencyPair.ToCurrency)
                return true;
            return false;
        }
    }
}
