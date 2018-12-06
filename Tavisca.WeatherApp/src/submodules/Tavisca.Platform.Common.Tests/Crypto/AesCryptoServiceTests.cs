using System;
using System.Linq;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.Crypto;
using Moq;
using Xunit;
using Tavisca.Platform.Common.Crypto;
using System.Security;
using System.IO;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.LockManagement;
using System.Threading;
using Tavisca.Platform.Common.ApplicationEventBus;
using Tavisca.Common.Plugins.Aws.KMS;
using Tavisca.Common.Plugins.Aws.S3;
using System.Collections.Specialized;
using System.Text;

namespace Tavisca.Platform.Common.Tests.Crypto
{
    public class AesCryptoServiceTests
    {
        private string _application = "application";
        private string _tenant = "demo";
        private string _key = "test_key";

        [Fact]
        public async Task EncryptAsync_WithInvalidInputs_ShouldThrowException()
        {
            var textToEncrypt = "text for encryption";

            var encrypter = new AesCryptoService(null);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await encrypter.EncryptAsync(null, _tenant, _key, textToEncrypt));

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await encrypter.EncryptAsync(_application, null, _key, textToEncrypt));

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await encrypter.EncryptAsync(_application, _tenant, null, textToEncrypt));

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await encrypter.EncryptAsync(_application, _tenant, _key, ""));
        }

        [Fact]
        public void Encrypt_WithInvalidInputs_Should_Throw_Exception()
        {
            var textToEncrypt = "text for encryption";

            var encrypter = new AesCryptoService(null);
            Assert.Throws<ArgumentNullException>(() => encrypter.Encrypt(null, _tenant, _key, textToEncrypt));

            Assert.Throws<ArgumentNullException>(() => encrypter.Encrypt(_application, null, _key, textToEncrypt));

            Assert.Throws<ArgumentNullException>(() => encrypter.Encrypt(_application, _tenant, null, textToEncrypt));

            Assert.Throws<ArgumentNullException>(() => encrypter.Encrypt(_application, _tenant, _key, ""));
        }

        [Fact]
        public async Task EncryptAsync_ShouldReturn_EncryptedText()
        {
            var textToEncrypt = "text for encryption";

            var provider = new Mock<ICryptoKeyProvider>();
            provider.Setup(x => x.GenerateCryptoKeyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(ConvertStreamToSecureString(new MemoryStream(Guid.NewGuid().ToByteArray())));

            var factory = new Mock<ICryptoKeyProviderFactory>();
            factory.Setup(x => x.GetKeyProviderAsync()).ReturnsAsync(provider.Object);

            var encrypter = new AesCryptoService(factory.Object);
            var encryptedText = await encrypter.EncryptAsync(_application, _tenant, _key, textToEncrypt);

            Assert.NotNull(encryptedText);
            Assert.NotEmpty(encryptedText);
        }
        [Fact]
        public void Encrypt_ShouldReturn_EncryptedText()
        {

            var textToEncrypt = "text for encryption";

            var provider = new Mock<ICryptoKeyProvider>();
            provider.Setup(x => x.GenerateCryptoKey(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(ConvertStreamToSecureString(new MemoryStream(Guid.NewGuid().ToByteArray())));

            var factory = new Mock<ICryptoKeyProviderFactory>();
            factory.Setup(x => x.GetKeyProvider()).Returns(provider.Object);

            var encrypter = new AesCryptoService(factory.Object);
            var encryptedText = encrypter.Encrypt(_application, _tenant, _key, textToEncrypt);

            Assert.NotNull(encryptedText);
            Assert.NotEmpty(encryptedText);
        }
         
        [Fact]
        public async Task DecryptAsync_WithInvalidInputs_ShouldThrowException()
        {
            var textToEncrypt = "text for encryption";

            var encrypter = new AesCryptoService(null);
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await encrypter.DecryptAsync(null, _tenant, _key, textToEncrypt));

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await encrypter.DecryptAsync(_application, null, _key, textToEncrypt));

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await encrypter.DecryptAsync(_application, _tenant, null, textToEncrypt));

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await encrypter.DecryptAsync(_application, _tenant, _key, ""));
        }

        [Fact]
        public void Decrypt_WithInvalidInputs_ShouldThrowException()
        {
            var textToEncrypt = "text for encryption";

            var encrypter = new AesCryptoService(null);
            Assert.Throws<ArgumentNullException>(() => encrypter.Decrypt(null, _tenant, _key, textToEncrypt));

            Assert.Throws<ArgumentNullException>(() => encrypter.Decrypt(_application, null, _key, textToEncrypt));

            Assert.Throws<ArgumentNullException>(() => encrypter.Decrypt(_application, _tenant, null, textToEncrypt));

            Assert.Throws<ArgumentNullException>(() => encrypter.Decrypt(_application, _tenant, _key, ""));
        }

        [Fact]
        public async Task DecryptAsync_ShouldReturn_OriginalPlainText()
        {
            var textToEncrypt = "Encrypt Text";

            var dataKey = new MemoryStream(Guid.NewGuid().ToByteArray());

            var cryptoKeyProvider = new Mock<ICryptoKeyProvider>();
            cryptoKeyProvider.Setup(x => x.GenerateCryptoKeyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(ConvertStreamToSecureString(dataKey));

            cryptoKeyProvider.Setup(x => x.GetCryptoKeyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(ConvertStreamToSecureString(dataKey));

            var cryptoKeyProviderFactory = new Mock<ICryptoKeyProviderFactory>();
            cryptoKeyProviderFactory.Setup(x => x.GetKeyProviderAsync()).ReturnsAsync(cryptoKeyProvider.Object);

            var encrypter = new AesCryptoService(cryptoKeyProviderFactory.Object);
            var encryptedText = await encrypter.EncryptAsync(_application, _tenant, _key, textToEncrypt);

            Assert.NotNull(encryptedText);
            Assert.NotEmpty(encryptedText);

            var decryptedText = await encrypter.DecryptAsync(_application, _tenant, _key, encryptedText);

            Assert.NotNull(decryptedText);
            Assert.NotEmpty(decryptedText);

            Assert.Equal(textToEncrypt, decryptedText);
        }


        [Fact]
        public async Task DecryptAsync_ShouldReturn_OriginalBytes()
        {
            var textToEncrypt = "Encrypt Text";

            var dataKey = new MemoryStream(Guid.NewGuid().ToByteArray());

            var cryptoKeyProvider = new Mock<ICryptoKeyProvider>();
            cryptoKeyProvider.Setup(x => x.GenerateCryptoKeyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(ConvertStreamToSecureString(dataKey));

            cryptoKeyProvider.Setup(x => x.GetCryptoKeyAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(ConvertStreamToSecureString(dataKey));

            var cryptoKeyProviderFactory = new Mock<ICryptoKeyProviderFactory>();
            cryptoKeyProviderFactory.Setup(x => x.GetKeyProviderAsync()).ReturnsAsync(cryptoKeyProvider.Object);

            var encrypter = new AesCryptoService(cryptoKeyProviderFactory.Object);
            var encryptedBytes = await encrypter.EncryptAsync(_application, _tenant, _key, GetBytes(textToEncrypt));

            Assert.NotNull(encryptedBytes);
            Assert.NotEmpty(encryptedBytes);

            var decryptedBytes = await encrypter.DecryptAsync(_application, _tenant, _key, encryptedBytes);

            Assert.NotNull(decryptedBytes);
            Assert.NotEmpty(decryptedBytes);

            Assert.Equal(textToEncrypt, GetStringFromBytes(decryptedBytes));
        }
        private byte[] GetBytes(string text)
        {
            return Encoding.UTF8.GetBytes(text);
        }

        private string GetStringFromBytes(byte[] value)
        {
            return Encoding.UTF8.GetString(value);
        }

        [Fact]
        public void Decrypt_ShouldReturn_OriginalPlainText()
        {
            var textToEncrypt = "Encrypt Text";

            var dataKey = new MemoryStream(Guid.NewGuid().ToByteArray());

            var cryptoKeyProvider = new Mock<ICryptoKeyProvider>();
            cryptoKeyProvider.Setup(x => x.GenerateCryptoKey(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(ConvertStreamToSecureString(dataKey));

            cryptoKeyProvider.Setup(x => x.GetCryptoKey(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(ConvertStreamToSecureString(dataKey));

            var cryptoKeyProviderFactory = new Mock<ICryptoKeyProviderFactory>();
            cryptoKeyProviderFactory.Setup(x => x.GetKeyProvider()).Returns(cryptoKeyProvider.Object);

            var encrypter = new AesCryptoService(cryptoKeyProviderFactory.Object);
            var encryptedText = encrypter.Encrypt(_application, _tenant, _key, textToEncrypt);

            Assert.NotNull(encryptedText);
            Assert.NotEmpty(encryptedText);

            var decryptedText = encrypter.Decrypt(_application, _tenant, _key, encryptedText);

            Assert.NotNull(decryptedText);
            Assert.NotEmpty(decryptedText);

            Assert.Equal(textToEncrypt, decryptedText);
        }

        [Fact(Skip ="Run this test to actually test with aws service")]
        public async Task AesCryptoService_Async_Tests()
        {
            //update keyIdentifier value 
            var keyIdentifier = "test_aws_kms";
            var plainText = "Encrypt data using AES and data key";

            var config = new Mock<IConfigurationProvider>();

            //Note setup aws profile on system or add aocfiguration 
            config.Setup(x => x.GetTenantConfigurationAsNameValueCollectionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync( new NameValueCollection());

            config.Setup(x => x.GetTenantConfigurationAsStringAsync(It.IsAny<string>(), "data_encryption", "s3_bucket_name")).
                ReturnsAsync("qa_data_encryption");

            var lockProvider = new Mock<ILockProvider>();
            lockProvider.Setup(x => x.TryGetLockAsync(It.IsAny<string>(), LockType.Write, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var cryptoService = new AesCryptoService(new CryptoKeyProviderFactory(config.Object, new KmsClientFactory(config.Object),
                new CryptoKeyStore(config.Object, new S3StoreFactory(config.Object)), lockProvider.Object));

            var encryptedText = await cryptoService.EncryptAsync("test_application", _tenant, keyIdentifier, plainText);

            var decryptedPlainText = await cryptoService.DecryptAsync("test_application", _tenant, keyIdentifier, encryptedText);

            Assert.Equal(plainText, decryptedPlainText);
        }

        [Fact(Skip = "Run this test to actually test with aws service")]
        public void Aes_CryptService_Sync_Test()
        {
            var keyIdentifier = "test_aws_kms";
            var plainText = "Encrypt data using AES and data key";

            var config = new Mock<IConfigurationProvider>();

            //Note setup aws profile on system or add aocfiguration 
            config.Setup(x => x.GetTenantConfigurationAsNameValueCollection(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new NameValueCollection());

            config.Setup(x => x.GetTenantConfigurationAsString(It.IsAny<string>(), "data_encryption", "s3_bucket_name")).
                Returns("qa_data_encryption");

            var lockProvider = new Mock<ILockProvider>();
            lockProvider.Setup(x => x.TryGetLockAsync(It.IsAny<string>(), LockType.Write, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var cryptoService = new AesCryptoService(new CryptoKeyProviderFactory(config.Object, new KmsClientFactory(config.Object),
                new CryptoKeyStore(config.Object, new S3StoreFactory(config.Object)), lockProvider.Object));

            var encryptedText = cryptoService.Encrypt("test_application", _tenant, keyIdentifier, plainText);

            var decryptedPlainText = cryptoService.Decrypt("test_application", _tenant, keyIdentifier, encryptedText);

            Assert.Equal(plainText, decryptedPlainText);
        }

        [Fact(Skip = "Run this test to actually test with aws service")]
        public async Task AesCryptoService_Bytes_Async_Tests()
        {
            //update keyIdentifier value 
            var keyIdentifier = "test_aws_kms";
            var plainText = "Encrypt data using AES and data key";

            var config = new Mock<IConfigurationProvider>();

            //Note setup aws profile on system or add aocfiguration 
            config.Setup(x => x.GetTenantConfigurationAsNameValueCollectionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new NameValueCollection());

            config.Setup(x => x.GetTenantConfigurationAsStringAsync(It.IsAny<string>(), "data_encryption", "s3_bucket_name")).
                ReturnsAsync("qa_data_encryption");

            var lockProvider = new Mock<ILockProvider>();
            lockProvider.Setup(x => x.TryGetLockAsync(It.IsAny<string>(), LockType.Write, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var cryptoService = new AesCryptoService(new CryptoKeyProviderFactory(config.Object, new KmsClientFactory(config.Object),
                new CryptoKeyStore(config.Object, new S3StoreFactory(config.Object)), lockProvider.Object));

            var encryptedBytes = await cryptoService.EncryptAsync("test_application", _tenant, keyIdentifier, GetBytes(plainText));

            var decryptedPlainBytes = await cryptoService.DecryptAsync("test_application", _tenant, keyIdentifier, encryptedBytes);

            Assert.Equal(plainText, GetStringFromBytes(decryptedPlainBytes));
        }

        private SecureString ConvertStreamToSecureString(MemoryStream plainTextBlob)
        {
            var plainText = Convert.ToBase64String(plainTextBlob.ToArray());

            SecureString secPlainText = new SecureString();
            plainText.ToCharArray().ToList().ForEach((q) => secPlainText.AppendChar(q));

            plainText = null;
            return secPlainText;
        }
    }
}
