using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.ExchangeRate;
using Tavisca.Common.Plugins.ExchangeRate.Exceptions;
using Tavisca.Common.Plugins.ExchangeRate.Entities;

namespace Tavisca.Common.Tests
{
    [TestClass]
    public class CurrencyConverterTest
    {

        private CurrencyConverter GetCurrencyConverter()
        {
            return new CurrencyConverter(new DummyExchangeRateProvider());
        }


        [TestMethod]
        public void GetConversionRateForSameCurrencyShouldReturnOne()
        {
            var currencyConverter = GetCurrencyConverter();
            var exchangeRate = currencyConverter.GetExchangeRate(new CurrencyPair("INR", "INR"));
            Assert.AreEqual(exchangeRate, 1);
        }

        [TestMethod]
        public void GetConversionRateForCurrencyPairHavingZeroRateShouldReturnZero()
        {
            var currencyConverter = GetCurrencyConverter();
            var exchangeRate = currencyConverter.GetExchangeRate(new CurrencyPair("USD", "AUD"));
            Assert.AreEqual(exchangeRate, 0);

        }


        [TestMethod]
        public  void GetConversionRateForValidCurrencyPairShouldReturnProperValue()
        {
            var currencyConverter = GetCurrencyConverter();
            var exchangeRate = currencyConverter.GetExchangeRate(new CurrencyPair("INR", "USD"));
            Assert.IsTrue(exchangeRate > 0);
            Assert.AreEqual(exchangeRate, 0.015M);
            exchangeRate = currencyConverter.GetExchangeRate(new CurrencyPair("EUR", "INR"));
            Assert.IsTrue(exchangeRate > 0);
            Assert.AreEqual(exchangeRate, 74);
            exchangeRate = currencyConverter.GetExchangeRate(new CurrencyPair("USD", "EUR"));
            Assert.IsTrue(exchangeRate > 0);
            Assert.AreEqual(exchangeRate, 0.90M);

        }


        [TestMethod]
        public void GetReverseConversionRateForValidCurrencyPairShouldReturProperValue()
        {
            var currencyConverter = GetCurrencyConverter();
            var exchangeRate = currencyConverter.GetExchangeRate(new CurrencyPair("USD", "INR"));
            Assert.IsTrue(exchangeRate > 0);
            Assert.AreEqual(exchangeRate, 1 / 0.015M);
            exchangeRate =  currencyConverter.GetExchangeRate(new CurrencyPair("INR", "EUR"));
            Assert.IsTrue(exchangeRate > 0);
            Assert.AreEqual(exchangeRate, 1M / 74);
            exchangeRate = currencyConverter.GetExchangeRate(new CurrencyPair("EUR", "USD"));
            Assert.IsTrue(exchangeRate > 0);
            Assert.AreEqual(exchangeRate, 1 / 0.90M);

        }

        [TestMethod]
        public  void GetOneToManyCurrencyConversionRatesShouldReturProperValue()
        {
            var currencyConverter = GetCurrencyConverter();
            var toCurrencies = new System.Collections.Generic.List<string>() { "USD", "EUR" };
            var exchangeRates = currencyConverter.GetAllExchangeRates("INR", toCurrencies);
            Assert.IsTrue(exchangeRates.Count == toCurrencies.Count);
            var exchangeRate = exchangeRates.Find(e => e.CurrencyPair.ToCurrency == "USD");
            Assert.IsNotNull(exchangeRate);
            Assert.AreEqual(exchangeRate.Rate, 0.015M);
            exchangeRate = exchangeRates.Find(e => e.CurrencyPair.ToCurrency == "EUR");
            Assert.IsNotNull(exchangeRate);
            Assert.AreEqual(exchangeRate.Rate, 1M / 74);
        }

        [TestMethod]
        public  void GetaManyToOneCurrencyConversionRatesShouldReturProperValue()
        {
            var currencyConverter = GetCurrencyConverter();
            var fromCurrencies = new System.Collections.Generic.List<string>() { "USD", "INR" };
            var exchangeRates = currencyConverter.GetAllExchangeRates(fromCurrencies, "EUR");
            Assert.IsTrue(exchangeRates.Count == fromCurrencies.Count);
            var exchangeRate = exchangeRates.Find(e => e.CurrencyPair.FromCurrency == "USD");
            Assert.IsNotNull(exchangeRate);
            Assert.AreEqual(exchangeRate.Rate, 0.90M);
            exchangeRate = exchangeRates.Find(e => e.CurrencyPair.FromCurrency == "INR");
            Assert.IsNotNull(exchangeRate);
            Assert.AreEqual(exchangeRate.Rate, 1M / 74);
        }

        [TestMethod]
        public void GetConvertedAmoutForCurrencyPairShouldReturProperValue()
        {
            var currencyConverter = GetCurrencyConverter();
            var exchangeRate = currencyConverter.Convert(100, new CurrencyPair("INR", "USD"));
            Assert.IsTrue(exchangeRate > 0);
            Assert.AreEqual(exchangeRate, 0.015M * 100);
            exchangeRate =  currencyConverter.Convert(100, new CurrencyPair("INR", "INR"));
            Assert.AreEqual(exchangeRate, 100);
        }



        [TestMethod]
        public void GetAllExchangeRatesShouldSkipInvalidCurrencies()
        {
            var currencyPairs = new List<CurrencyPair>()
            {
               new CurrencyPair("INR","USD"),
               new CurrencyPair("INR","ABC")
            };

            var currencyConverter = GetCurrencyConverter();
            var exchangeRates = currencyConverter.GetAllExchangeRates(currencyPairs);
            Assert.AreEqual(exchangeRates.Count, 1);

        }


        [TestMethod]
        public void TestNullCurrencyShouldThrowException()
        {
             Xunit.Assert.Throws<ArgumentNullException>(() =>new CurrencyPair(null, "USD"));
        }

        [TestMethod]
        public void GetConversionRateForInvalidCurrencyPairShouldThrowException()
        {
            var currencyConverter = GetCurrencyConverter();
             Xunit.Assert.Throws<ArgumentException>(()=>  currencyConverter.GetExchangeRate(new CurrencyPair("INR", "MDL")));

        }

        [TestMethod]
        public void GetConversionRateForCurrencyPairHavingNoRateShouldThrowException()
        {
            var currencyConverter = GetCurrencyConverter();
            Xunit.Assert.Throws<ExchangeRateNotFoundException>(() =>  currencyConverter.GetExchangeRate(new CurrencyPair("INR", "AUD")));

        }
    }
}
