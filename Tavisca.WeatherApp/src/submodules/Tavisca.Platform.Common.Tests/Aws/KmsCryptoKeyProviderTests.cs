using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Moq;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.Aws;
using Tavisca.Common.Plugins.Aws.KMS;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.ExceptionManagement;
using Tavisca.Platform.Common.LockManagement;
using Xunit;

namespace Tavisca.Platform.Common.Tests.Aws
{
    public class KmsCryptoKeyProviderTests
    {
        private string _application = "application";
        private string _tenant = "demo";

        public KmsCryptoKeyProviderTests()
        {
            ExceptionPolicy.Configure(new TestErrorHandler());
        }

        [Fact]
        public async Task GenerateCryptoKeyAsync_NoDataInRemoteStore_ShouldReturn_NewCryptoKey()
        {
            var config = new Mock<IConfigurationProvider>();
            var keyBytes = Guid.NewGuid().ToByteArray();
            var key = Guid.NewGuid().ToString();

            var awsClient = new Mock<IAmazonKeyManagementService>();
            awsClient.Setup(x => x.GenerateDataKeyAsync(It.IsAny<GenerateDataKeyRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(GetDataKeyResponse(keyBytes));

            var kmsClientFactory = new Mock<IKmsClientFactory>();
            kmsClientFactory.Setup(x => x.GetGlobalClientAsync()).ReturnsAsync(awsClient.Object);

            var cryptoKeyStore = new Mock<ICryptoKeyStore>();
            cryptoKeyStore.Setup(x => x.GetAsync(_application, _tenant, key)).ReturnsAsync(null);

            var lockProvider = new Mock<ILockProvider>();
            lockProvider.Setup(x => x.TryGetLockAsync(It.IsAny<string>(), LockType.Write, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var kmsCryptoKeyProvider = new KmsCryptoKeyProvider(config.Object, kmsClientFactory.Object, cryptoKeyStore.Object, lockProvider.Object);

            var dataKey = await kmsCryptoKeyProvider.GenerateCryptoKeyAsync(_application, _tenant, key);

            Assert.NotNull(dataKey);
        }

        [Fact]
        public void GenerateCryptoKey_NoDataInRemoteStore_ShouldReturn_NewCryptoKey()
        {
            var config = new Mock<IConfigurationProvider>();
            var keyBytes = Guid.NewGuid().ToByteArray();
            var key = Guid.NewGuid().ToString();

            var awsClient = new Mock<IAmazonKeyManagementService>();
            awsClient.Setup(x => x.GenerateDataKeyAsync(It.IsAny<GenerateDataKeyRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(GetDataKeyResponse(keyBytes));

            var kmsClientFactory = new Mock<IKmsClientFactory>();
            kmsClientFactory.Setup(x => x.GetGlobalClient()).Returns(awsClient.Object);

            var cryptoKeyStore = new Mock<ICryptoKeyStore>();
            cryptoKeyStore.Setup(x => x.Get(_application, _tenant, key)).Returns(() => null);

            var lockProvider = new Mock<ILockProvider>();
            lockProvider.Setup(x => x.TryGetLockAsync(It.IsAny<string>(), LockType.Write, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var kmsCryptoKeyProvider = new KmsCryptoKeyProvider(config.Object, kmsClientFactory.Object, cryptoKeyStore.Object, lockProvider.Object);

            var dataKey = kmsCryptoKeyProvider.GenerateCryptoKey(_application, _tenant, key);

            Assert.NotNull(dataKey);
        }

        [Fact]
        public async Task GenerateCryptoKeyAsync_KmsRetrunsError_ShouldThrowException()
        {
            var config = new Mock<IConfigurationProvider>();
            var keyBytes = Guid.NewGuid().ToByteArray();
            var key = Guid.NewGuid().ToString();

            var kmsResponse = GetDataKeyResponse(keyBytes);
            kmsResponse.HttpStatusCode = HttpStatusCode.InternalServerError;

            var awsClient = new Mock<IAmazonKeyManagementService>();
            awsClient.Setup(x => x.GenerateDataKeyAsync(It.IsAny<GenerateDataKeyRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(kmsResponse);

            var kmsClientFactory = new Mock<IKmsClientFactory>();
            kmsClientFactory.Setup(x => x.GetGlobalClientAsync()).ReturnsAsync(awsClient.Object);

            var cryptoKeyStore = new Mock<ICryptoKeyStore>();
            cryptoKeyStore.Setup(x => x.GetAsync(_application, _tenant, key)).ReturnsAsync(null);

            var lockProvider = new Mock<ILockProvider>();
            lockProvider.Setup(x => x.TryGetLockAsync(It.IsAny<string>(), LockType.Write, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var kmsCryptoKeyProvider = new KmsCryptoKeyProvider(config.Object, kmsClientFactory.Object, cryptoKeyStore.Object, lockProvider.Object);

            var dataKey = await Assert.ThrowsAsync<CommunicationException>(async () => await kmsCryptoKeyProvider.GenerateCryptoKeyAsync(_application, _tenant, key));

            Assert.Equal(dataKey.ErrorCode, FaultCodes.KMSCommunicationError);
        }

        [Fact]
        public void GenerateCryptoKey_KmsRetrunsError_ShouldThrowException()
        {
            var config = new Mock<IConfigurationProvider>();
            var keyBytes = Guid.NewGuid().ToByteArray();
            var key = Guid.NewGuid().ToString();

            var kmsResponse = GetDataKeyResponse(keyBytes);
            kmsResponse.HttpStatusCode = HttpStatusCode.InternalServerError;

            var awsClient = new Mock<IAmazonKeyManagementService>();
            awsClient.Setup(x => x.GenerateDataKey(It.IsAny<GenerateDataKeyRequest>())).Returns(kmsResponse);

            var kmsClientFactory = new Mock<IKmsClientFactory>();
            kmsClientFactory.Setup(x => x.GetGlobalClient()).Returns(awsClient.Object);

            var cryptoKeyStore = new Mock<ICryptoKeyStore>();
            cryptoKeyStore.Setup(x => x.Get(_application, _tenant, key)).Returns(() => null);

            var lockProvider = new Mock<ILockProvider>();
            lockProvider.Setup(x => x.TryGetLockAsync(It.IsAny<string>(), LockType.Write, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var kmsCryptoKeyProvider = new KmsCryptoKeyProvider(config.Object, kmsClientFactory.Object, cryptoKeyStore.Object, lockProvider.Object);

            var dataKey = Assert.Throws<CommunicationException>(() => kmsCryptoKeyProvider.GenerateCryptoKey(_application, _tenant, key));

            Assert.Equal(dataKey.ErrorCode, FaultCodes.KMSCommunicationError);
        }

        [Fact]
        public async Task GenerateCryptoKeyAsync_DataInRemoteStore_ShouldReturn_CryptoKey()
        {
            var config = new Mock<IConfigurationProvider>();
            var keyBytesInCryptoKeyStore = Guid.NewGuid().ToByteArray();
            var key = Guid.NewGuid().ToString();

            var awsClient = new Mock<IAmazonKeyManagementService>();
            awsClient.Setup(x => x.GenerateDataKeyAsync(It.IsAny<GenerateDataKeyRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetDataKeyResponse(Guid.NewGuid().ToByteArray()));

            awsClient.Setup(x => x.DecryptAsync(It.IsAny<DecryptRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetDecryptResponse(keyBytesInCryptoKeyStore));

            var kmsClientFactory = new Mock<IKmsClientFactory>();
            kmsClientFactory.Setup(x => x.GetGlobalClientAsync()).ReturnsAsync(awsClient.Object);

            var cryptoKeyStore = new Mock<ICryptoKeyStore>();
            cryptoKeyStore.Setup(x => x.GetAsync(_application, _tenant, key)).ReturnsAsync(keyBytesInCryptoKeyStore);

            var lockProvider = new Mock<ILockProvider>();
            lockProvider.Setup(x => x.TryGetLockAsync(It.IsAny<string>(), LockType.Write, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var kmsCryptoKeyProvider = new KmsCryptoKeyProvider(config.Object, kmsClientFactory.Object, cryptoKeyStore.Object, lockProvider.Object);

            var dataKey = await kmsCryptoKeyProvider.GenerateCryptoKeyAsync(_application, _tenant, key);

            Assert.NotNull(dataKey);
            Assert.Equal(DataKeyHelper.ConvertSecureStrToString(dataKey), DataKeyHelper.ConvertBytesToString(keyBytesInCryptoKeyStore));
        }

        [Fact]
        public void GenerateCryptoKey_DataInRemoteStore_ShouldReturn_CryptoKey()
        {
            var config = new Mock<IConfigurationProvider>();
            var keyBytesInCryptoKeyStore = Guid.NewGuid().ToByteArray();
            var key = Guid.NewGuid().ToString();

            var awsClient = new Mock<IAmazonKeyManagementService>();
            awsClient.Setup(x => x.GenerateDataKeyAsync(It.IsAny<GenerateDataKeyRequest>(), It.IsAny<CancellationToken>()))
                .Returns(() => { return Task.FromResult(GetDataKeyResponse(Guid.NewGuid().ToByteArray())); });


            awsClient.Setup(x => x.DecryptAsync(It.IsAny<DecryptRequest>(), It.IsAny<CancellationToken>()))
                .Returns(() => { return Task.FromResult(GetDecryptResponse(keyBytesInCryptoKeyStore)); });

            var kmsClientFactory = new Mock<IKmsClientFactory>();
            kmsClientFactory.Setup(x => x.GetGlobalClient()).Returns(awsClient.Object);

            var cryptoKeyStore = new Mock<ICryptoKeyStore>();
            cryptoKeyStore.Setup(x => x.Get(_application, _tenant, key)).Returns(keyBytesInCryptoKeyStore);

            var lockProvider = new Mock<ILockProvider>();
            lockProvider.Setup(x => x.TryGetLockAsync(It.IsAny<string>(), LockType.Write, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var kmsCryptoKeyProvider = new KmsCryptoKeyProvider(config.Object, kmsClientFactory.Object, cryptoKeyStore.Object, lockProvider.Object);

            var dataKey = kmsCryptoKeyProvider.GenerateCryptoKey(_application, _tenant, key);

            Assert.NotNull(dataKey);
            Assert.Equal(DataKeyHelper.ConvertSecureStrToString(dataKey), DataKeyHelper.ConvertBytesToString(keyBytesInCryptoKeyStore));

        }

        [Fact]
        public async Task GetCryptoKeyAsync_NoKeyInRemoteStore_ShouldThrowException()
        {
            var config = new Mock<IConfigurationProvider>();
            var keyBytesInCryptoKeyStore = Guid.NewGuid().ToByteArray();
            var key = Guid.NewGuid().ToString();

            var awsClient = new Mock<IAmazonKeyManagementService>();
            awsClient.Setup(x => x.DecryptAsync(It.IsAny<DecryptRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetDecryptResponse(keyBytesInCryptoKeyStore));

            var kmsClientFactory = new Mock<IKmsClientFactory>();
            kmsClientFactory.Setup(x => x.GetGlobalClientAsync()).ReturnsAsync(awsClient.Object);

            var cryptoKeyStore = new Mock<ICryptoKeyStore>();
            cryptoKeyStore.Setup(x => x.GetAsync(_application, _tenant, key)).ReturnsAsync(null);

            var lockProvider = new Mock<ILockProvider>();
            lockProvider.Setup(x => x.TryGetLockAsync(It.IsAny<string>(), LockType.Write, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var kmsCryptoKeyProvider = new KmsCryptoKeyProvider(config.Object, kmsClientFactory.Object, cryptoKeyStore.Object, lockProvider.Object);

            var response = await Assert.ThrowsAsync<Tavisca.Common.Plugins.Aws.SystemException>(async () => await kmsCryptoKeyProvider.GetCryptoKeyAsync(_application, _tenant, key));

            Assert.Equal(response.ErrorCode, FaultCodes.CryptoKeyNotFound);
        }

        [Fact]
        public void GetCryptoKey_NoKeyInRemoteStore_ShouldThrowException()
        {
            var config = new Mock<IConfigurationProvider>();
            var keyBytesInCryptoKeyStore = Guid.NewGuid().ToByteArray();
            var key = Guid.NewGuid().ToString();

            var awsClient = new Mock<IAmazonKeyManagementService>();
            awsClient.Setup(x => x.Decrypt(It.IsAny<DecryptRequest>()))
                .Returns(GetDecryptResponse(keyBytesInCryptoKeyStore));

            var kmsClientFactory = new Mock<IKmsClientFactory>();
            kmsClientFactory.Setup(x => x.GetGlobalClient()).Returns(awsClient.Object);

            var cryptoKeyStore = new Mock<ICryptoKeyStore>();
            cryptoKeyStore.Setup(x => x.Get(_application, _tenant, key)).Returns(() => null);

            var lockProvider = new Mock<ILockProvider>();
            lockProvider.Setup(x => x.TryGetLockAsync(It.IsAny<string>(), LockType.Write, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var kmsCryptoKeyProvider = new KmsCryptoKeyProvider(config.Object, kmsClientFactory.Object, cryptoKeyStore.Object, lockProvider.Object);

            var response = Assert.Throws<Tavisca.Common.Plugins.Aws.SystemException>(() => kmsCryptoKeyProvider.GetCryptoKey(_application, _tenant, key));

            Assert.Equal(response.ErrorCode, FaultCodes.CryptoKeyNotFound);
        }

        [Fact]
        public async Task GetCryptoKeyAsync_KeyInRemoteStore_ShouldReturn_CryptoKey()
        {
            byte[] keyFromRemote = null;
            var config = new Mock<IConfigurationProvider>();
            var keyBytesInCryptoKeyStore = Guid.NewGuid().ToByteArray();
            var key = Guid.NewGuid().ToString();

            var awsClient = new Mock<IAmazonKeyManagementService>();
            awsClient.Setup(x => x.DecryptAsync(It.IsAny<DecryptRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetDecryptResponse(keyBytesInCryptoKeyStore));

            var kmsClientFactory = new Mock<IKmsClientFactory>();
            kmsClientFactory.Setup(x => x.GetGlobalClientAsync()).ReturnsAsync(awsClient.Object);

            var cryptoKeyStore = new Mock<ICryptoKeyStore>();
            cryptoKeyStore.Setup(x => x.GetAsync(_application, _tenant, key))
                .Callback<string, string, string>((app, tenant, keyId) => keyFromRemote = keyBytesInCryptoKeyStore)
                .ReturnsAsync(keyBytesInCryptoKeyStore);

            var lockProvider = new Mock<ILockProvider>();
            lockProvider.Setup(x => x.TryGetLockAsync(It.IsAny<string>(), LockType.Write, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var kmsCryptoKeyProvider = new KmsCryptoKeyProvider(config.Object, kmsClientFactory.Object, cryptoKeyStore.Object, lockProvider.Object);

            var dataKey = await kmsCryptoKeyProvider.GenerateCryptoKeyAsync(_application, _tenant, key);

            Assert.NotNull(dataKey);
            Assert.Equal(DataKeyHelper.ConvertSecureStrToString(dataKey), DataKeyHelper.ConvertBytesToString(keyBytesInCryptoKeyStore));
        }

        [Fact]
        public void GetCryptoKey_KeyInRemoteStore_ShouldReturn_CryptoKey()
        {
            byte[] keyFromRemote = null;
            var config = new Mock<IConfigurationProvider>();
            var keyBytesInCryptoKeyStore = Guid.NewGuid().ToByteArray();
            var key = Guid.NewGuid().ToString();

            var awsClient = new Mock<IAmazonKeyManagementService>();
            awsClient.Setup(x => x.DecryptAsync(It.IsAny<DecryptRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetDecryptResponse(keyBytesInCryptoKeyStore));

            var kmsClientFactory = new Mock<IKmsClientFactory>();
            kmsClientFactory.Setup(x => x.GetGlobalClient()).Returns(awsClient.Object);

            var cryptoKeyStore = new Mock<ICryptoKeyStore>();
            cryptoKeyStore.Setup(x => x.Get(_application, _tenant, key))
                .Callback<string, string, string>((app, tenant, keyId) => keyFromRemote = keyBytesInCryptoKeyStore)
                .Returns(keyBytesInCryptoKeyStore);

            var lockProvider = new Mock<ILockProvider>();
            lockProvider.Setup(x => x.TryGetLockAsync(It.IsAny<string>(), LockType.Write, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var kmsCryptoKeyProvider = new KmsCryptoKeyProvider(config.Object, kmsClientFactory.Object, cryptoKeyStore.Object, lockProvider.Object);

            var dataKey = kmsCryptoKeyProvider.GenerateCryptoKey(_application, _tenant, key);

            Assert.NotNull(dataKey);
            Assert.Equal(DataKeyHelper.ConvertSecureStrToString(dataKey), DataKeyHelper.ConvertBytesToString(keyBytesInCryptoKeyStore));
        }

        private static DecryptResponse GetDecryptResponse(byte[] keyBytesInCryptoKeyStore)
        {
            return new DecryptResponse { HttpStatusCode = HttpStatusCode.OK, Plaintext = new MemoryStream(keyBytesInCryptoKeyStore), ResponseMetadata = new Amazon.Runtime.ResponseMetadata() };
        }

        private static GenerateDataKeyResponse GetDataKeyResponse(byte[] keyBytes)
        {
            return new GenerateDataKeyResponse
            {
                Plaintext = new MemoryStream(keyBytes),
                CiphertextBlob = new MemoryStream(keyBytes),
                HttpStatusCode = HttpStatusCode.OK,
                ResponseMetadata = new Amazon.Runtime.ResponseMetadata()
            };
        }
    }
}
