using Moq;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.Crypto;
using Tavisca.Platform.Common.Crypto;
using Xunit;

namespace Tavisca.Platform.Common.Tests.Crypto
{
    public class CachedCryptoKeyProviderTests
    {
        private string _application = "application";
        private string _tenant = "demo";

        [Fact]
        public async Task GenerateCryptoKeyAsync_NoDataInCahce_RetrunFromRemote_UpdateCache()
        {
            var dataKey = GetSecureDataKey();
            var newDataKey = GetSecureDataKey();
            var key = Guid.NewGuid().ToString();

            var provider = new Mock<ICryptoKeyProvider>();
            provider.Setup(store => store.GenerateCryptoKeyAsync(_application, _tenant, key))
            .ReturnsAsync(dataKey);

            var cachedProvider = new CachedCryptoKeyProvider(provider.Object);

            var value = await cachedProvider.GenerateCryptoKeyAsync(_application, _tenant, key);
            Assert.True(IsEqual(dataKey, value));
            
            provider.Setup(store => store.GenerateCryptoKeyAsync(_application, _tenant, key))
            .ReturnsAsync(newDataKey);

            value = await cachedProvider.GenerateCryptoKeyAsync(_application, _tenant, key);

            Assert.True(IsEqual(dataKey, value));
            Assert.False(IsEqual(newDataKey, value));
        }
        [Fact]
        public void GenerateCryptoKey_NoDataInCache_ReturnFromRemote_UpdateCache()
        {
            var dataKey = GetSecureDataKey();
            var newDataKey = GetSecureDataKey();
            var key = Guid.NewGuid().ToString();

            var provider = new Mock<ICryptoKeyProvider>();
            provider.Setup(store => store.GenerateCryptoKey(_application, _tenant, key))
            .Returns(dataKey);

            var cachedProvider = new CachedCryptoKeyProvider(provider.Object);

            var value = cachedProvider.GenerateCryptoKey(_application, _tenant, key);
            Assert.True(IsEqual(dataKey, value));

            provider.Setup(store => store.GenerateCryptoKey(_application, _tenant, key))
            .Returns(newDataKey);

            value = cachedProvider.GenerateCryptoKey(_application, _tenant, key);

            Assert.True(IsEqual(dataKey, value));
            Assert.False(IsEqual(newDataKey, value));
        }

        [Fact]
        public async Task GetCryptoKeyAsync_NoDataInCahce_RetrunFromRemote()
        {
            var dataKey = GetSecureDataKey();
            var provider = new Mock<ICryptoKeyProvider>();
            var key = Guid.NewGuid().ToString();

            var cachedProvider = new CachedCryptoKeyProvider(provider.Object);

            provider.Setup(store => store.GetCryptoKeyAsync(_application, _tenant, key))
            .ReturnsAsync(dataKey);

            var value = await cachedProvider.GetCryptoKeyAsync(_application, _tenant, key);

            Assert.True(IsEqual(dataKey, value));
        }

        [Fact]
        public void GetCryptoKey_NoDataInCache_ReturnFromRemote()
        {
            var dataKey = GetSecureDataKey();
            var provider = new Mock<ICryptoKeyProvider>();
            var key = Guid.NewGuid().ToString();

            var cachedProvider = new CachedCryptoKeyProvider(provider.Object);

            provider.Setup(store => store.GetCryptoKey(_application, _tenant, key))
            .Returns(dataKey);

            var value = cachedProvider.GetCryptoKey(_application, _tenant, key);

            Assert.True(IsEqual(dataKey, value));
        }

        [Fact]
        public async Task GetCryptoKeyAsync_NoDataInCahce_RetrunFromRemote_UpdateCache()
        {
            var dataKey = GetSecureDataKey();
            var newDataKey = GetSecureDataKey();
            var key = Guid.NewGuid().ToString();

            var provider = new Mock<ICryptoKeyProvider>();
            provider.Setup(store => store.GetCryptoKeyAsync(_application, _tenant, key))
            .ReturnsAsync(dataKey);

            var cachedProvider = new CachedCryptoKeyProvider(provider.Object);

            var value = await cachedProvider.GetCryptoKeyAsync(_application, _tenant, key);
            Assert.True(IsEqual(dataKey, value));

            provider.Setup(store => store.GetCryptoKeyAsync(_application, _tenant, key))
            .ReturnsAsync(newDataKey);
            value = await cachedProvider.GetCryptoKeyAsync(_application, _tenant, key);

            Assert.True(IsEqual(dataKey, value));
            Assert.False(IsEqual(newDataKey, value));
        }
        [Fact]
        public void GetCryptoKey_NoDataInCache_ReturnFromRemote_UpdateCache()
        {
            var dataKey = GetSecureDataKey();
            var newDataKey = GetSecureDataKey();
            var key = Guid.NewGuid().ToString();

            var provider = new Mock<ICryptoKeyProvider>();
            provider.Setup(store => store.GetCryptoKey(_application, _tenant, key))
            .Returns(dataKey);

            var cachedProvider = new CachedCryptoKeyProvider(provider.Object);

            var value = cachedProvider.GetCryptoKey(_application, _tenant, key);
            Assert.True(IsEqual(dataKey, value));

            provider.Setup(store => store.GetCryptoKey(_application, _tenant, key))
            .Returns(newDataKey);
            value = cachedProvider.GetCryptoKey(_application, _tenant, key);

            Assert.True(IsEqual(dataKey, value));
            Assert.False(IsEqual(newDataKey, value));
        }

        [Fact]
        public async Task GetCryptoKeyAsync_NoDataInCahce_RetrunFromRemote_DoNotUpdateCacheIfNull()
        {
            var key = Guid.NewGuid().ToString();
            var dataKey = GetSecureDataKey();

            var provider = new Mock<ICryptoKeyProvider>();
            provider.Setup(store => store.GetCryptoKeyAsync(_application, _tenant, key))
            .ReturnsAsync(null);

            var cachedProvider = new CachedCryptoKeyProvider(provider.Object);

            var value = await cachedProvider.GetCryptoKeyAsync(_application, _tenant, key);
            Assert.Null(value);

            provider.Setup(store => store.GetCryptoKeyAsync(_application, _tenant, key))
            .ReturnsAsync(dataKey);

            value = await cachedProvider.GetCryptoKeyAsync(_application, _tenant, key);
            Assert.True(IsEqual(dataKey, value));
        }

        [Fact]
        public void GetCryptoKey_NoDataInCahce_RetrunFromRemote_DoNotUpdateCacheIfNull()
        {
            var key = Guid.NewGuid().ToString();
            var dataKey = GetSecureDataKey();

            var provider = new Mock<ICryptoKeyProvider>();
            provider.Setup(store => store.GetCryptoKey(_application, _tenant, key))
            .Returns(() => null);

            var cachedProvider = new CachedCryptoKeyProvider(provider.Object);

            var value = cachedProvider.GetCryptoKey(_application, _tenant, key);
            Assert.Null(value);

            provider.Setup(store => store.GetCryptoKey(_application, _tenant, key))
            .Returns(dataKey);

            value = cachedProvider.GetCryptoKey(_application, _tenant, key);
            Assert.True(IsEqual(dataKey, value));
        }

        private SecureString GetSecureDataKey()
        {
            SecureString secPlainText = new SecureString();
            Guid.NewGuid().ToString().ToCharArray().ToList().ForEach((q) => secPlainText.AppendChar(q));

            return secPlainText;
        }

        private bool IsEqual(SecureString secString1, SecureString secString2)
        {
            var stringA = ConvertSecureStrToString(secString1);
            var stringB = ConvertSecureStrToString(secString2);

            return stringA.Equals(stringB);
        }

        private string ConvertSecureStrToString(SecureString secureKey)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureKey);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}
