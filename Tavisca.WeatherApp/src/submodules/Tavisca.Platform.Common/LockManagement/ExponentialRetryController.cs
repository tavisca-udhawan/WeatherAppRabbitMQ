using System;
using Tavisca.Platform.Common.ConfigurationHandler;

namespace Tavisca.Platform.Common.LockManagement
{
    public class ExponentialRetryController : IRetryController
    {
        private readonly DateTime _retryTimeLimit;
        private int _retryCount;
        public ExponentialRetryController()
        {
            var value = ConfigurationManager.GetAppSetting("LockRetryTimeout");
            int interval;
            if (!int.TryParse(value, out interval))
                interval = 10000;

            _retryTimeLimit = DateTime.UtcNow.Add(TimeSpan.FromMilliseconds(interval));
        }

        public int? GetNextRetryInterval()
        {
            if (DateTime.UtcNow >= _retryTimeLimit)
                return null;

            _retryCount++;

            var interval = (int)Math.Pow(3, _retryCount) * 10;
            return interval;
        }
    }
}
