using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Tavisca.Common.Plugins.ExchangeRate.Entities;
using Tavisca.Common.Plugins.ExchangeRate.Exceptions;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common;
using System.Linq;

namespace Tavisca.Common.Plugins.ExchangeRate.Providers
{
    public class PerodicExchangeRateService : IExchangeRateService
    {
        private static Dictionary<CurrencyPair, decimal>[] _exchangeRates;

        private static HashSet<string> _supportedCurrencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly IExchangeRateRepository _exchangeRateRepository;

        private readonly IConfigurationProvider _configurationProvider;

        private int _numberOfSlots;

        private static int _currentSlot;

        private static int _previousSlot;

        private static bool _isFirstLoadSuccess;

        private static System.Threading.Timer _dateRefreshTimer;

        static object _lock = new object();

        public int BootstrapTimeIntervalInSecs = 300;

        private static DateTime _timerLastStartTime;

        public PerodicExchangeRateService(IExchangeRateRepository exchangeRateRepository, IConfigurationProvider configurationProvider, int slots = 6)
        {
            this._exchangeRateRepository = exchangeRateRepository;
            this._configurationProvider = configurationProvider;
            _numberOfSlots = slots;
            IntializeSettings();

        }

        private void IntializeSettings()
        {
            var waitHandle = new ManualResetEvent(false);
            if (_exchangeRates == null)
            {
                lock (_lock)
                {
                    if (_exchangeRates == null)
                    {
                        // 1. Create empty dictnaoaries
                        SetUpExchangeList();
                        Task.Factory.StartNew(async () =>
                        {
                            try
                            {
                                await FirstLoad();
                            }
                            finally
                            {
                                waitHandle.Set();
                            }
                        });
                        waitHandle.WaitOne();
                        // 4. Set refersh Timer
                        IntializeRefreshTimer();
                    }
                }
            }

        }

        private async Task FirstLoad()
        {
            // 1. Load data if dict is empty
            await LoadExchangeRatesAsync();
            // 2. Set previous slot in accordance to current slot for the first time
            LoadPreviousSlot();

            _isFirstLoadSuccess = true;

        }

        private void LoadPreviousSlot()
        {
            if (_currentSlot != _previousSlot)
                _previousSlot = _currentSlot - 1;

            var currentFeed = _exchangeRates[_currentSlot];

            _exchangeRates[_previousSlot] = new Dictionary<CurrencyPair, decimal>(currentFeed);
        }

        private void SetUpExchangeList()
        {
            int slotWithBackup = _numberOfSlots + 1;
            _currentSlot = 0;
            _previousSlot = 0;

            _exchangeRates = new Dictionary<CurrencyPair, decimal>[slotWithBackup];
            for (int i = 0; i < slotWithBackup; i++)
            {
                _exchangeRates[i] = new Dictionary<CurrencyPair, decimal>();
            }
        }

        public decimal GetExchangeRate(string fromCurrency, string toCurrency)
        {

            if (StringComparer.OrdinalIgnoreCase.Equals(fromCurrency, toCurrency) == true)
                return 1;

            decimal rate;

            var exchangeRate = GetApplicableRates();

            if (exchangeRate.TryGetValue(new CurrencyPair(fromCurrency, toCurrency), out rate))
                return rate;

            throw new ExchangeRateNotFoundException(fromCurrency, toCurrency);
        }

        public IEnumerable<string> GetSupportedCurrencies()
        {
            return new HashSet<string>(_supportedCurrencies);
        }

        private Dictionary<CurrencyPair, decimal> GetApplicableRates()
        {

            var isBootStrapedPeriodPassed = (DateTime.UtcNow.Subtract(_timerLastStartTime).TotalSeconds
                                            > BootstrapTimeIntervalInSecs);


            return isBootStrapedPeriodPassed ? _exchangeRates[_currentSlot] : _exchangeRates[_previousSlot];
        }

        private async Task LoadExchangeRatesAsync()
        {
            _timerLastStartTime = DateTime.UtcNow;
            try
            {
                var rates = await _exchangeRateRepository.GetCurrencyConversionRatesAsync();

                if (rates == null)
                    throw new ExchangeRateNotFoundException("No content from Dunamo DB");

                var currentSlot = GetCurrentSlot();
                _exchangeRates[currentSlot] = new Dictionary<CurrencyPair, decimal>(rates);
                _previousSlot = _currentSlot;
                _currentSlot = currentSlot;

                //Load supported currencies
                _supportedCurrencies = LoadSupportedCurrencies(_exchangeRates[currentSlot]);
            }
            catch (Exception ex)
            {
                if (_isFirstLoadSuccess == false)
                    _exchangeRates = null;

                Platform.Common.ExceptionPolicy.HandleException(ex, "logonly");
            }
        }

        private int GetCurrentSlot()
        {
            int minutesInOneSlot = Math.Max(1440 / _numberOfSlots, 1);

            int totalMiutesSinceDayStart = (DateTime.UtcNow.Hour * 60) + DateTime.UtcNow.Minute;

            int currentSlot = totalMiutesSinceDayStart / minutesInOneSlot;

            return currentSlot;
        }

        private void IntializeRefreshTimer()
        {
            var dueTime = GetFirstTimeInterval();

            var period = GetNextTimeInterval();

            _dateRefreshTimer = new System.Threading.Timer(o => LoadExchangeRatesAsync().Wait(), null, dueTime, period);

        }

        private int GetNextTimeInterval()
        {

            int interval = GetRefreshInterval();

            // Converted return value in mili seconds minutes * 60 * 1000
            return (interval * 60 * 1000);

        }

        private int GetFirstTimeInterval()
        {
            // Check configuration for a value or set as 12hr default
            int interval = GetRefreshInterval();

            // Converted return value in mili seconds minutes * 60 * 1000
            return (CalculateFirstTimerInterval(interval) * 60 * 1000);
        }

        private int GetRefreshInterval()
        {
            decimal interval = Math.Max(1440 / _numberOfSlots, 1);

            return Convert.ToInt32(interval);
        }



        private int CalculateFirstTimerInterval(int scheduledInterval)
        {
            int minutesInADay = 1440;

            int timeElapsedForDayInMin = (DateTime.UtcNow.Hour * 60) + DateTime.UtcNow.Minute;

            int numberOfMinutesLeftForNextSlot = scheduledInterval - (timeElapsedForDayInMin % scheduledInterval);

            int numberOfMinutesleftInDayEnd = minutesInADay - timeElapsedForDayInMin;

            // Need to reset all clocks at start of UTC day
            int nextInterval = Math.Min(numberOfMinutesleftInDayEnd, numberOfMinutesLeftForNextSlot);

            // Cannot set to less than 1 minute
            return Math.Max(nextInterval, 1);
        }

        private HashSet<string> LoadSupportedCurrencies(Dictionary<CurrencyPair, decimal> exchangeRates)
        {
            try
            {
                if (exchangeRates == null)
                    return null;

                var currencyCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var exchangeRate in exchangeRates.Keys)
                    currencyCodes.Add(exchangeRate.FromCurrency);

                return currencyCodes;
            }
            catch (Exception ex)
            {
                Platform.Common.ExceptionPolicy.HandleException(ex, "logonly");
            }

            return null;
        }
    }
}
