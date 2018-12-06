using Moq;
using System;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.Aws.KMS;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.FileStore;
using Xunit;

namespace Tavisca.Platform.Common.Tests.Aws
{
    public class CryptoKeyStoreTests
    {
        private string _tenant = "demo";
        private string _application = "application";

        [Fact]
        public async Task CryptoKeyStore_AddAsync()
        {
            var bytes = Guid.NewGuid().ToByteArray();
            byte[] bytesToReturn = null;
            var key = Guid.NewGuid().ToString();

            var config = new Mock<IConfigurationProvider>();
            config.Setup(x => x.GetGlobalConfigurationAsStringAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync("test_bucket");

            var fileStore = new Mock<IFileStore>();
            fileStore.Setup(x => x.AddAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, string, byte[]>((k, p, v) => bytesToReturn = v).Returns(Task.CompletedTask);

            fileStore.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(bytesToReturn);

            var fileStoreFactory = new Mock<IFileStoreFactory>();
            fileStoreFactory.Setup(x => x.GetGlobalClientAsync()).ReturnsAsync(fileStore.Object);

            var cryptoKeyStore = new CryptoKeyStore(config.Object, fileStoreFactory.Object);

            await cryptoKeyStore.AddAsync(_application, _tenant, key, bytes);

            var value = cryptoKeyStore.GetAsync(_application, _tenant, key);

            Assert.NotNull(value);
        }

        [Fact]
        public void CryptoKeyStore_Add()
        {
            var bytes = Guid.NewGuid().ToByteArray();
            byte[] bytesToReturn = null;
            var key = Guid.NewGuid().ToString();

            var config = new Mock<IConfigurationProvider>();
            config.Setup(x => x.GetGlobalConfigurationAsString(It.IsAny<string>(), It.IsAny<string>()))
                .Returns("test_bucket");

            var fileStore = new Mock<IFileStore>();
            fileStore.Setup(x => x.Add(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, string, byte[]>((k, p, v) => bytesToReturn = v);

            fileStore.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(() => bytesToReturn);

            var fileStoreFactory = new Mock<IFileStoreFactory>();
            fileStoreFactory.Setup(x => x.GetGlobalClient()).Returns(fileStore.Object);

            var cryptoKeyStore = new CryptoKeyStore(config.Object, fileStoreFactory.Object);

            cryptoKeyStore.Add(_application, _tenant, key, bytes);

            var value = cryptoKeyStore.Get(_application, _tenant, key);

            Assert.NotNull(value);
        }

        [Fact]
        public async Task CryptoKeyStore_GetAsync_NoDataInStore_ShouldReturn_Null()
        {
            var key = Guid.NewGuid().ToString();
            var config = new Mock<IConfigurationProvider>();
            config.Setup(x => x.GetGlobalConfigurationAsStringAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("test_bucket");

            var fileStore = new Mock<IFileStore>();
            fileStore.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null);

            var fileStoreFactory = new Mock<IFileStoreFactory>();
            fileStoreFactory.Setup(x => x.GetGlobalClientAsync()).ReturnsAsync(fileStore.Object);

            var cryptoKeyStore = new CryptoKeyStore(config.Object, fileStoreFactory.Object);

            var value = await cryptoKeyStore.GetAsync(_application, _tenant, key);

            Assert.Null(value);
        }

        [Fact]
        public void CryptoKeyStore_Get_NoDataInStore_ShouldReturn_Null()
        {
            var key = Guid.NewGuid().ToString();
            var config = new Mock<IConfigurationProvider>();
            config.Setup(x => x.GetGlobalConfigurationAsString(It.IsAny<string>(), It.IsAny<string>())).Returns("test_bucket");

            var fileStore = new Mock<IFileStore>();
            fileStore.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(()=> null);

            var fileStoreFactory = new Mock<IFileStoreFactory>();
            fileStoreFactory.Setup(x => x.GetGlobalClient()).Returns(fileStore.Object);

            var cryptoKeyStore = new CryptoKeyStore(config.Object, fileStoreFactory.Object);

            var value = cryptoKeyStore.Get(_application, _tenant, key);

            Assert.Null(value);
        }

        [Fact]
        public async Task CryptoKeyStore_GetAsync_DataInStore_ShouldReturn_Data()
        {
            var bytes = Guid.NewGuid().ToByteArray();
            var key = Guid.NewGuid().ToString();
            var config = new Mock<IConfigurationProvider>();
            config.Setup(x => x.GetGlobalConfigurationAsStringAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("test_bucket");

            var fileStore = new Mock<IFileStore>();
            fileStore.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(bytes);

            var fileStoreFactory = new Mock<IFileStoreFactory>();
            fileStoreFactory.Setup(x => x.GetGlobalClientAsync()).ReturnsAsync(fileStore.Object);

            var cryptoKeyStore = new CryptoKeyStore(config.Object, fileStoreFactory.Object);

            var value = await cryptoKeyStore.GetAsync(_application, _tenant, key);

            Assert.NotNull(value);
        }

        [Fact]
        public void CryptoKeyStore_Get_DataInStore_ShouldReturn_Data()
        {
            var bytes = Guid.NewGuid().ToByteArray();
            var key = Guid.NewGuid().ToString();
            var config = new Mock<IConfigurationProvider>();
            config.Setup(x => x.GetGlobalConfigurationAsString(It.IsAny<string>(), It.IsAny<string>())).Returns("test_bucket");

            var fileStore = new Mock<IFileStore>();
            fileStore.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>())).Returns(bytes);

            var fileStoreFactory = new Mock<IFileStoreFactory>();
            fileStoreFactory.Setup(x => x.GetGlobalClient()).Returns(fileStore.Object);

            var cryptoKeyStore = new CryptoKeyStore(config.Object, fileStoreFactory.Object);

            var value = cryptoKeyStore.Get(_application, _tenant, key);

            Assert.NotNull(value);
        }
    }
}
