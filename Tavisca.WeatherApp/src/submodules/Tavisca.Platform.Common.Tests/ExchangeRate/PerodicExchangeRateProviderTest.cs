using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.IO;
using Tavisca.Common.Plugins.ExchangeRate.Providers;
using Tavisca.Common.Plugins.ExchangeRate.Entities;
using Tavisca.Common.Plugins.ExchangeRate.Exceptions;
using Tavisca.Platform.Common;
using System.Collections.Generic;

namespace Tavisca.Common.Tests
{
    [TestClass]
    public class PerodicExchangeRateProviderTest
    {
        private static HashSet<string> _currencyCodes = new HashSet<string> { "INR", "USD", "EUR" };

        private PerodicExchangeRateService GetRefreshingExchangeRateProvider()
        {
            var configStore = new DummyConfigurationStore();
            configStore.Configuration = new System.Collections.Specialized.NameValueCollection();
            configStore.Configuration.Add(Constants.NumberOfSlots, (24 * 60).ToString());

            return new PerodicExchangeRateService(new RefreshingDBRepository(), configStore,1440);
        }

        private IExchangeRateService GetExchangeRateProviderWithEmptyConfig()
        {
            var configStore = new DummyConfigurationStore();
            configStore.Configuration = new System.Collections.Specialized.NameValueCollection();
            configStore.Configuration.Add(Constants.NumberOfSlots, string.Empty);

            return new PerodicExchangeRateService(new RefreshingDBRepository(), configStore);
        }

        private IExchangeRateService GetExchangeRateProviderWithInvalidConfig()
        {
            var configStore = new DummyConfigurationStore();
            configStore.Configuration = new System.Collections.Specialized.NameValueCollection();
            configStore.Configuration.Add(Constants.NumberOfSlots, "abc");

            return new PerodicExchangeRateService(new RefreshingDBRepository(), configStore);
        }


        [TestMethod]
        public void GetExchangeRateForValidCurrenyPair()
        {
            var exchangeRateProvider = GetRefreshingExchangeRateProvider();
            var exchangeRate = exchangeRateProvider.GetExchangeRate("EUR", "INR");

            Assert.IsNotNull(exchangeRate);

        }

        [TestMethod]
        public  void ExchangeRateNotFoundThrowException()
        {
            var exchangeRateProvider = GetRefreshingExchangeRateProvider();

            Xunit.Assert.Throws<ExchangeRateNotFoundException>(() => exchangeRateProvider.GetExchangeRate("ABC", "AUD"));

        }

        [TestMethod]
        public void ExchangeRateNotUpdatedWithinRefreshCycle()
        {
            var exchangeRateProvider = GetRefreshingExchangeRateProvider();

            var actualRate1 = exchangeRateProvider.GetExchangeRate("EUR", "INR");

            Thread.Sleep(10000);

            var actualRate2 = exchangeRateProvider.GetExchangeRate("EUR", "INR");

            Assert.AreEqual(actualRate1, actualRate2);

        }

        [TestMethod]
        public void ExchangeRateMultiInstanceShareData()
        {
            var exchangeRateProvider1 = GetRefreshingExchangeRateProvider();

            var actualRate1 = exchangeRateProvider1.GetExchangeRate("EUR", "INR");

            Thread.Sleep(2000);

            var exchangeRateProvider2 = GetRefreshingExchangeRateProvider();

            var actualRate2 = exchangeRateProvider2.GetExchangeRate("EUR", "INR");

            Assert.AreEqual(actualRate1, actualRate2);

        }

        [Timeout(100000)]

        [TestMethod]
        public void ExchangeRateUpdatedAfterRefreshCycle()
        {
            var exchangeRateProvider = GetRefreshingExchangeRateProvider();

            exchangeRateProvider.BootstrapTimeIntervalInSecs = 2;


            var actualRate1 = exchangeRateProvider.GetExchangeRate("EUR", "INR");

            Thread.Sleep(60000);

            Thread.Sleep(5000);


            var actualRate2 = exchangeRateProvider.GetExchangeRate("EUR", "INR");

            Assert.AreNotEqual(actualRate1, actualRate2);

        }


        [TestMethod]
        public void ExchageRateRefreshIntervalSetDefaultForEmptyConfig()
        {
            var exchangeRateProvider = GetExchangeRateProviderWithEmptyConfig();
            var exchangeRate = exchangeRateProvider.GetExchangeRate("EUR", "INR");

            Assert.IsNotNull(exchangeRate);

        }


        [TestMethod]
        public void ExchageRateRefreshIntervalSetDefaultForInvalidConfig()
        {
            var exchangeRateProvider = GetExchangeRateProviderWithInvalidConfig();
            var exchangeRate = exchangeRateProvider.GetExchangeRate("EUR", "INR");

            Assert.IsNotNull(exchangeRate);

        }

        [TestMethod]
        public void GetCurrencyCodesForValidCurrenyPair()
        {
            var exchangeRateProvider = GetRefreshingExchangeRateProvider();
            var currencyCodes = exchangeRateProvider.GetSupportedCurrencies();
            
            Xunit.Assert.NotNull(currencyCodes);
            Xunit.Assert.True(new HashSet<string>(currencyCodes).SetEquals(_currencyCodes));
        }

        [TestMethod]
        public void GetCurrencyCodesForValidCurrenyPair_UpdateCurrenciesList_ShouldReturnOriginalList()
        {
            var exchangeRateProvider = GetRefreshingExchangeRateProvider();
            var currencyCodes = exchangeRateProvider.GetSupportedCurrencies();

            Xunit.Assert.NotNull(currencyCodes);
            Xunit.Assert.True(new HashSet<string>(currencyCodes).SetEquals(_currencyCodes));

            ((HashSet<string>)(currencyCodes)).Add("ABC");

            var currencyCodes1 = exchangeRateProvider.GetSupportedCurrencies();

            Xunit.Assert.NotNull(currencyCodes1);
            Xunit.Assert.True(new HashSet<string>(currencyCodes1).SetEquals(_currencyCodes));

        }

        [TestMethod]
        public void GetCurrencyCodes_ExchangeRateMultiInstanceShareData()
        {
            var exchangeRateProvider = GetRefreshingExchangeRateProvider();

            var currencyCodes1 = exchangeRateProvider.GetSupportedCurrencies();
            Xunit.Assert.NotNull(currencyCodes1);
            Thread.Sleep(2000);

            var currencyCodes2 = exchangeRateProvider.GetSupportedCurrencies();
            Xunit.Assert.NotNull(currencyCodes2);
            Xunit.Assert.True(new HashSet<string>(currencyCodes1).SetEquals(currencyCodes2));
        }
    }
}

 
