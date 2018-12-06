using System.Threading.Tasks;
using Tavisca.Common.Plugins.Aws;
using Tavisca.Common.Plugins.Aws.KMS;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.Crypto;
using Tavisca.Platform.Common.LockManagement;

namespace Tavisca.Common.Plugins.Crypto
{
    public class CryptoKeyProviderFactory : ICryptoKeyProviderFactory
    {
        private readonly IConfigurationProvider _config;
        private readonly ICryptoKeyStore _cryptoKeyStore;
        private readonly ILockProvider _lockProvider;
        private readonly IKmsClientFactory _kmsClientFactory;
        public CryptoKeyProviderFactory(IConfigurationProvider config, IKmsClientFactory kmsClientFactory, ICryptoKeyStore cryptoKeyStore, ILockProvider lockProvider)
        {
            _config = config;
            _cryptoKeyStore = cryptoKeyStore;
            _lockProvider = lockProvider;
            _kmsClientFactory = kmsClientFactory;
        }

        public ICryptoKeyProvider GetKeyProvider()
        {
            return new KmsCryptoKeyProvider(_config, _kmsClientFactory, _cryptoKeyStore, _lockProvider)
                .WithCaching();
        }
        public async Task<ICryptoKeyProvider> GetKeyProviderAsync()
        {
            return new KmsCryptoKeyProvider(_config, _kmsClientFactory, _cryptoKeyStore, _lockProvider)
                .WithCaching();
        }
    }

    public static class CryptoKeyProviderExtension
    {
        public static ICryptoKeyProvider WithCaching(this ICryptoKeyProvider client)
        {
            return new CachedCryptoKeyProvider(client);
        }
    }
}
