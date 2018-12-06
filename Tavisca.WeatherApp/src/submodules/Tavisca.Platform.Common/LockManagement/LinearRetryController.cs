
using Tavisca.Platform.Common.ConfigurationHandler;

namespace Tavisca.Platform.Common.LockManagement
{
    public class LinearRetryController : IRetryController
    {
        private int _retryCount;
        
        public LinearRetryController()
        {
            if (!int.TryParse(ConfigurationManager.GetAppSetting("LockRetryCount"), out _retryCount))
                _retryCount = 30;
        }

        public int? GetNextRetryInterval()
        {
            if (_retryCount == 0)
                return null;

            int retryInterval;
            _retryCount--;
            return !int.TryParse(ConfigurationManager.GetAppSetting("LockRetryIntervalInMilliSeconds"), out retryInterval)
                ? 100
                : retryInterval;
        }
    }
}
