using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.LockManagement;
using Tavisca.Platform.Common.Profiling;
using Tavisca.Common.Plugins.Redis;
using ServiceStack.Redis;
using Tavisca.Platform.Common;

namespace Tavisca.Common.Plugins.Redis
{
    public class RedisLockProvider : ILockProvider
    {
        private IRedisClient GetClient(RedisLockSettings settings)
        {
            return _clientFactory.Create(settings);
        }



        readonly IRedisClientFactory _clientFactory;
        readonly ILockSettingsProvider _redisLockSettings;



        public RedisLockProvider(IRedisClientFactory redisFactory, ILockSettingsProvider redisLockSettings)
        {
            _clientFactory = redisFactory;
            _redisLockSettings = redisLockSettings;


        }
        public async Task ReleaseLockAsync(string lockId, LockType lockType, CancellationToken cancellationToken)
        {
            using (new ProfileContext("Releasing Lock"))
            {
                try
                {
                    var settings = await _redisLockSettings.GetSettings() as RedisLockSettings;

                    using (var client = GetClient(settings))
                    {
                        int retryCount = 3;
                        Exception ex = null;
                        do
                        {
                            try
                            {
                                client.Remove(lockId);
                                ex = null;   //nullify the exception if  after the retry lock is provided
                                break;
                            }
                            catch (Exception exception)
                            {
                                ex = exception;
                                retryCount--;
                            }
                        }
                        while (retryCount > 0);

                        if (ex != null)
                            throw ex;
                    }
                }
                catch (Exception ex)
                {
                    throw new RedisProviderException(Keystore.RedisKeys.Error, ex);
                }
            }
        }



        public async Task<bool> TryGetLockAsync(string lockId, LockType lockType, CancellationToken cancellationToken)
        {
            try
            {
                var settings = await _redisLockSettings.GetSettings() as RedisLockSettings;
                using (var client = GetClient(settings))

                {

                    RetryUntilTrue(
                        () =>
                        {
                            //get timeput from configuration if null set the default timeout 
                            var timout = settings.LockTimeOut == 0 ? 60 : settings.LockTimeOut;
                            //Calculate a unix time for when the lock should expire
                            DateTime expireTime = DateTime.UtcNow.AddSeconds(timout);
                            string lockString = (expireTime.ToUnixTimestamp() + 1).ToString();
                            int retryCount = 3;
                            Exception ex = null;

                            bool flag = false;

                            //retry logic to get  lock
                            do
                            {
                                try
                                {
                                    //Try to set the lock, if it does not exist this will succeed and the lock is obtained
                                    flag = client.SetValueIfNotExists(lockId, lockString);
                                    ex = null;              //nullify the exception if  after the retry lock is provided
                                    break;
                                }
                                catch (Exception exception)
                                {
                                    ex = exception;
                                    retryCount--;
                                }
                            }
                            while (retryCount > 0);   //retry untill the retry count is 0

                            if (flag == true)
                                return true;

                            if (ex != null)
                                throw ex;


                            //If we've gotten here then a key for the lock is present. This could be because the lock is
                            //correctly acquired or it could be because a client that had acquired the lock crashed (or didn't release it properly).
                            //Therefore we need to get the value of the lock to see when it should expire

                            client.Watch(lockId);
                            string lockExpireString = client.Get<string>(lockId);
                            long lockExpireTime;
                            if (!long.TryParse(lockExpireString, out lockExpireTime))
                            {
                                client.UnWatch();  // since the client is scoped externally
                                return false;
                            }

                            //If the expire time is greater than the current time then we can't let the lock go yet
                            if (lockExpireTime > DateTime.UtcNow.ToUnixTimestamp())
                            {
                                client.UnWatch();  // since the client is scoped externally
                                return false;
                            }

                            //If the expire time is less than the current time then it wasn't released properly and we can attempt to 
                            //acquire the lock. The above call to Watch(_lockKey) enrolled the key in monitoring, so if it changes
                            //before we call Commit() below, the Commit will fail and return false, which means that another thread 
                            //was able to acquire the lock before we finished processing.
                            using (var trans = client.CreateTransaction()) // we started the "Watch" above; this tx will succeed if the value has not moved 
                            {
                                trans.QueueCommand(r => r.Set(lockId, lockString));
                                return trans.Commit(); //returns false if Transaction failed
                            }
                        }, TimeSpan.FromSeconds(settings.RetryTimeOut == 0 ? 60 : settings.RetryTimeOut));
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new RedisProviderException(Keystore.RedisKeys.Error, ex);

            }
        }

        private static void RetryUntilTrue(Func<bool> action, TimeSpan? timeOut)
        {
            var firstAttempt = DateTime.UtcNow;

            while (timeOut == null || DateTime.UtcNow - firstAttempt < timeOut.Value)
            {
                if (action())
                {
                    return;
                }
                // wait for 50 milliseconds before trying again
                Thread.Sleep(50);
            }

            throw new TimeoutException($"Exceeded timeout of {timeOut.Value}");
        }
    }
}
