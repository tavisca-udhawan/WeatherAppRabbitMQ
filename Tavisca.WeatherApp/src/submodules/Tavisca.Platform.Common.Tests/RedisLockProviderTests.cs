using ST = ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.Configuration;
using Tavisca.Common.Plugins.Redis;
using Tavisca.Platform.Common.LockManagement;
using Xunit;
using Moq;

namespace Tavisca.Platform.Common.Tests
{
    public class RedisLockProviderTests
    {

        [Fact]
        public async Task TestAcquireAndReleaseReadLock_Success()
        {
            var key = new Random().Next().ToString();
            var lockProvider = new RedisLockProvider(new RedisClientFactory(), GetSettingsProvider());

            var lockAcquireStatus = await lockProvider.TryGetLockAsync(key, LockType.Read, CancellationToken.None);
            Assert.True(lockAcquireStatus);
            var value = GetKeyValue(key);

            Assert.NotNull(value);
            await lockProvider.ReleaseLockAsync(key, LockType.Read, CancellationToken.None);
            value = GetKeyValue(key);
            Assert.Null(value);

        }

        private string GetKeyValue(string key)
        {
            var settingsProvider = GetSettingsProvider();
            var settings = settingsProvider.GetSettings().Result as RedisLockSettings;
            using (var client = new ST.PooledRedisClientManager(settings.Hosts.ToArray()))
            {
                var client1 = client.GetClient();
                var value = client1.GetValue(key);
                return value;
            }
        }

        [Fact]
        public async Task TestAcquireAndReleaseWriteLock_Success()
        {
            var key = new Random().Next().ToString();
            var lockProvider = new RedisLockProvider(new RedisClientFactory(), GetSettingsProvider());
            var lockAcquireStatus = await lockProvider.TryGetLockAsync(key, LockType.Write, CancellationToken.None);
            Assert.True(lockAcquireStatus);
            var value = GetKeyValue(key);
            Assert.NotNull(value);
            await lockProvider.ReleaseLockAsync(key, LockType.Write, CancellationToken.None);
            value = GetKeyValue(key);
            Assert.Null(value);

        }

        [Fact]
        public async Task TestRetryTimeOutExceptionForGettingWriteLockInCaseReadLocksExist_Failure()
        {
            var key = new Random().Next().ToString();

            var settingsProvider = new Mock<ILockSettingsProvider>();


            settingsProvider.Setup(x => x.GetSettings()).ReturnsAsync(new RedisLockSettings { Hosts = { "qa-redis.oski.tavisca.com:6379" }, QueueName = "QA-LoggingQueue", LockTimeOut = 20, RetryTimeOut = 10 });

            var lockProvider = new RedisLockProvider(new RedisClientFactory(), settingsProvider.Object);
            var readLockAcquireStatus = await lockProvider.TryGetLockAsync(key, LockType.Read, CancellationToken.None);
            Assert.True(readLockAcquireStatus);

            var ex = await Assert.ThrowsAsync<RedisProviderException>(() => lockProvider.TryGetLockAsync(key, LockType.Write, CancellationToken.None));
            Assert.NotNull(ex);
            Assert.Equal("Error occured in redis Provider", ex.Message);

        }


        [Fact]
        public async Task TestRetryTimeOutExceptionForGettingReadLockInCaseWriteLocksExist_Failure()
        {
            var key = new Random().Next().ToString();

            var settingsProvider = new Mock<ILockSettingsProvider>();


            settingsProvider.Setup(x => x.GetSettings()).ReturnsAsync(new RedisLockSettings { Hosts = { "qa-redis.oski.tavisca.com:6379" }, QueueName = "QA-LoggingQueue", LockTimeOut = 20, RetryTimeOut = 10 });

            var lockProvider = new RedisLockProvider(new RedisClientFactory(), settingsProvider.Object);
            var readLockAcquireStatus = await lockProvider.TryGetLockAsync(key, LockType.Write, CancellationToken.None);
            Assert.True(readLockAcquireStatus);


            var ex = await Assert.ThrowsAsync<RedisProviderException>(() => lockProvider.TryGetLockAsync(key, LockType.Read, CancellationToken.None));
            Assert.NotNull(ex);
            Assert.Equal("Error occured in redis Provider", ex.Message);

        }

        [Fact]
        public async Task TestForGettingReadLockInCaseWriteLocksExistBeforeRetryTimeOut_Success()
        {
            var key = new Random().Next().ToString();
            var redisSettings = GetSettingsProvider();
            var lockProvider = new RedisLockProvider(new RedisClientFactory(), redisSettings);
            var writeLockAcquireStatus = await lockProvider.TryGetLockAsync(key, LockType.Write, CancellationToken.None);
            Assert.True(writeLockAcquireStatus);

            var readLockAcquiresStatus = await lockProvider.TryGetLockAsync(key, LockType.Read, CancellationToken.None);
            Assert.True(readLockAcquiresStatus);

        }
        [Fact]
        public async Task TestForGettingWriteLockInCaseWriteReadLockExistBeforeRetryTimeOut_Success()
        {
            var key = new Random().Next().ToString();
            var redisSettings = GetSettingsProvider();
            //need to add timeout
            var lockProvider = new RedisLockProvider(new RedisClientFactory(), redisSettings);
            var readLockAcquireStatus = await lockProvider.TryGetLockAsync(key, LockType.Read, CancellationToken.None);
            Assert.True(readLockAcquireStatus);

            var wrtieLockAcquiresStatus = await lockProvider.TryGetLockAsync(key, LockType.Write, CancellationToken.None);
            Assert.True(wrtieLockAcquiresStatus);

        }

        private static Tavisca.Platform.Common.LockManagement.ILockSettingsProvider GetSettingsProvider()
        {
            return new RedisSettingsProvider(new ConfigurationProvider("test_application"));
        }

    }

}

