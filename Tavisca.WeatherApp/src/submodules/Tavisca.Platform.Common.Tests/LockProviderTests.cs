using Aerospike.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Tavisca.Common.Plugins.Aerospike;
using Tavisca.Common.Plugins.Configuration;
using Tavisca.Platform.Common.LockManagement;
using Xunit;
using System.Collections.Generic;

namespace Tavisca.Platform.Common.Tests
{
    public class LockProviderTests
    {
        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task TestAcquireAndReleaseReadLock()
        {
            var key = new Random().Next().ToString();
            var lockProvider = new AerospikeLockProvider(GetClientFactory(), GetSettingsProvider());
            try
            {
                var lockAcquireStatus = await lockProvider.TryGetLockAsync(key, LockType.Read, CancellationToken.None);
                Assert.True(lockAcquireStatus);
                var lockValue = await GetLockCount(lockProvider, key, LockType.Read);
                Assert.Equal(lockValue, 1);

                await lockProvider.ReleaseLockAsync(key, LockType.Read, CancellationToken.None);
                lockValue = await GetLockCount(lockProvider, key, LockType.Read);
                Assert.Equal(lockValue, 0);
            }
            finally
            {
                var client = await GetClient();
                client.Delete(null, await GetRecordKey(key));
            }
        }

        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task TestAcquireAndReleaseWriteLock()
        {
            var key = new Random().Next().ToString();
            var lockProvider = new AerospikeLockProvider(GetClientFactory(), GetSettingsProvider());
            try
            {
                var lockAcquireStatus = await lockProvider.TryGetLockAsync(key, LockType.Write, CancellationToken.None);
                Assert.True(lockAcquireStatus);
                var lockValue = await GetLockCount(lockProvider, key, LockType.Write);
                Assert.Equal(lockValue, 1);
                await lockProvider.ReleaseLockAsync(key, LockType.Write, CancellationToken.None);
                lockValue = await GetLockCount(lockProvider, key, LockType.Read);
                Assert.Equal(lockValue, 0);
            }
            finally
            {
                var client = await GetClient();
                client.Delete(null, await GetRecordKey(key));
            }
        }

        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task TestFailureForGettingWriteLockInCaseReadLocksExist()
        {
            var key = new Random().Next().ToString();
            var lockProvider = new AerospikeLockProvider(GetClientFactory(), GetSettingsProvider());
            try
            {
                var lockAcquireStatus = await lockProvider.TryGetLockAsync(key, LockType.Read, CancellationToken.None);
                Assert.True(lockAcquireStatus);

                lockAcquireStatus = await lockProvider.TryGetLockAsync(key, LockType.Write, CancellationToken.None);
                Assert.False(lockAcquireStatus);

                await lockProvider.ReleaseLockAsync(key, LockType.Read, CancellationToken.None);
                lockAcquireStatus = await lockProvider.TryGetLockAsync(key, LockType.Write, CancellationToken.None);
                Assert.True(lockAcquireStatus);
            }
            finally
            {
                var client = await GetClient();
                client.Delete(null, await GetRecordKey(key));
            }
        }

        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task TestFailureForGettingReadLockInCaseWriteLockExists()
        {
            var key = new Random().Next().ToString();
            var lockProvider = new AerospikeLockProvider(GetClientFactory(), GetSettingsProvider());
            try
            {
                var lockAcquireStatus = await lockProvider.TryGetLockAsync(key, LockType.Write, CancellationToken.None);
                Assert.True(lockAcquireStatus);

                lockAcquireStatus = await lockProvider.TryGetLockAsync(key, LockType.Read, CancellationToken.None);
                Assert.False(lockAcquireStatus);

                await lockProvider.ReleaseLockAsync(key, LockType.Write, CancellationToken.None);
                lockAcquireStatus = await lockProvider.TryGetLockAsync(key, LockType.Read, CancellationToken.None);
                Assert.True(lockAcquireStatus);
            }
            finally
            {
                var settings = await GetSettingsProvider().GetSettings();
                var client =  GetClientFactory().GetClient(settings.Host, settings.Port,new List<string>());
                client.Delete(null, await GetRecordKey(key));
            }
        }

        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task TryGetLockAsync_WhenSettingsAreChangedForAerospikeClient_ItShouldReflectInClientCreation()
        {
            var key = new Random().Next().ToString();

            var settingsProvider = new Mock<ILockProviderSettings>();

            var host1 = "172.16.3.91";
            var port1 = 3000;
            settingsProvider.Setup(x => x.GetSettings()).ReturnsAsync(new AerospikeSettings {Host = host1, Port =port1, Namespace = "lock", SecondaryNamespace = "bar", Set = "lock" });
            var lockProvider = new AerospikeLockProvider(GetClientFactory(), settingsProvider.Object);
            try
            {
                var lockAcquireStatus = await lockProvider.TryGetLockAsync(key, LockType.Write, CancellationToken.None);
                Assert.True(lockAcquireStatus);
                
                await lockProvider.ReleaseLockAsync(key, LockType.Write, CancellationToken.None);
                lockAcquireStatus = await lockProvider.TryGetLockAsync(key, LockType.Read, CancellationToken.None);
                Assert.True(lockAcquireStatus);


                settingsProvider.Setup(x => x.GetSettings()).ReturnsAsync(new AerospikeSettings { Host = "192.168.0.138", Port = port1, Namespace = "lock", SecondaryNamespace = "bar", Set = "lock"});
                 lockProvider = new AerospikeLockProvider(GetClientFactory(), settingsProvider.Object);

                var result = Assert.ThrowsAsync<AerospikeProviderException>(()=>lockProvider.TryGetLockAsync(key, LockType.Write, CancellationToken.None));
                Assert.NotNull(result);

            }
            finally
            {
                var settings = await GetSettingsProvider().GetSettings();
                var client =  GetClientFactory().GetClient(settings.Host, settings.Port, new List<string>());
                client.Delete(null, await GetRecordKey(key));
            }
        }
        #region private methods

        private static async Task<Key> GetRecordKey(string userKey)
        {
            var settingProvider = GetSettingsProvider();
            var settings = await settingProvider.GetSettings();
            return new Key(settings.Namespace, settings.Set, userKey);
        }

        private static async Task<int> GetLockCount(AerospikeLockProvider lockProvider, string key, LockType lockType)
        {
            var settingProvider = GetSettingsProvider();
            var settings = await settingProvider.GetSettings();
            var client =  GetClientFactory().GetClient(settings.Host, settings.Port,new List<string>());
            var record = client.Get(null,
                new Key(settings.Namespace, settings.Set, key));

            var binName = lockType == LockType.Read ?
                Keystore.AerospikeKeys.BinNames.ReadLocks :
                Keystore.AerospikeKeys.BinNames.WriteLocks;
            return record.GetInt(binName);
        }

        private static ILockProviderSettings _settingsProvider;
        private static ILockProviderSettings GetSettingsProvider()
        {
            return _settingsProvider ?? (_settingsProvider = new AerospikeSettingsProvider(new ConfigurationProvider("test_application")));
        }

        private static IAerospikeClientFactory GetClientFactory()
        {
            return new AerospikeClientFactory();
        }

        private static async Task<AsyncClient> GetClient()
        {
            var settings = await GetSettingsProvider().GetSettings();
            return  GetClientFactory().GetClient(settings.Host, settings.Port,new List<string>());
        }

        #endregion
    }
}
