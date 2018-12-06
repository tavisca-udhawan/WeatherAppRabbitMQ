using System.Security;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Crypto;

namespace Tavisca.Common.Plugins.Crypto
{
    public class CachedCryptoKeyProvider : ICryptoKeyProvider
    {
        private readonly ICryptoKeyProvider _remoteKeyStore;

        public CachedCryptoKeyProvider(ICryptoKeyProvider remoteKeyStore)
        {
            _remoteKeyStore = remoteKeyStore;
        }
        
        public SecureString GenerateCryptoKey(string application, string tenantId, string keyIdentifier)
        {
            var cacheKey = KeyHelper.ConstructKey(application, tenantId, keyIdentifier);
            return GetCryptoKeyFromCache(cacheKey) ?? GenerateKeyFromStore(application, tenantId, keyIdentifier, cacheKey);
        }

        public SecureString GetCryptoKey(string application, string tenantId, string keyIdentifier)
        {
            var cacheKey = KeyHelper.ConstructKey(application, tenantId, keyIdentifier);
            return GetCryptoKeyFromCache(cacheKey) ?? GetKeyFromStore(application, tenantId, keyIdentifier, cacheKey);
        }

        public async Task<SecureString> GenerateCryptoKeyAsync(string application, string tenantId, string keyIdentifier)
        {
            var cacheKey = KeyHelper.ConstructKey(application, tenantId, keyIdentifier);
            return GetCryptoKeyFromCache(cacheKey) ?? await GenerateKeyFromStoreAsync(application, tenantId, keyIdentifier, cacheKey);
        }
      
        public async Task<SecureString> GetCryptoKeyAsync(string application, string tenantId, string keyIdentifier)
        {
            var cacheKey = KeyHelper.ConstructKey(application, tenantId, keyIdentifier);
            return GetCryptoKeyFromCache(cacheKey) ?? await GetKeyFromStoreAsync(application, tenantId, keyIdentifier, cacheKey);
        }
        
        private static void UpdateCryptoKeyInCache(string cacheKey, SecureString cryptoKey)
        {
            if (cryptoKey != null)
                LocalCryptoKeyStore.Update(cacheKey, cryptoKey);
        }

        private static SecureString GetCryptoKeyFromCache(string cacheKey)
        {
            return LocalCryptoKeyStore.Get(cacheKey);
        }

        private async Task<SecureString> GenerateKeyFromStoreAsync(string application, string tenantId, string keyIdentifier, string cacheKey)
        {
            var cryptoKey = await _remoteKeyStore.GenerateCryptoKeyAsync(application, tenantId, keyIdentifier);
            UpdateCryptoKeyInCache(cacheKey, cryptoKey);
            return cryptoKey;
        }

        private SecureString GenerateKeyFromStore(string application, string tenantId, string keyIdentifier, string cacheKey)
        {
            var cryptoKey = _remoteKeyStore.GenerateCryptoKey(application, tenantId, keyIdentifier);
            if (cryptoKey != null)
                LocalCryptoKeyStore.Update(cacheKey, cryptoKey);
            return cryptoKey;
        }

        private async Task<SecureString> GetKeyFromStoreAsync(string application, string tenantId, string keyIdentifier, string cacheKey)
        {
            var cryptoKey = await _remoteKeyStore.GetCryptoKeyAsync(application, tenantId, keyIdentifier);
            if (cryptoKey != null)
                LocalCryptoKeyStore.Update(cacheKey, cryptoKey);
            return cryptoKey;
        }

        private SecureString GetKeyFromStore(string application, string tenantId, string keyIdentifier, string cacheKey)
        {
            var cryptoKey = _remoteKeyStore.GetCryptoKey(application, tenantId, keyIdentifier);
            UpdateCryptoKeyInCache(cacheKey, cryptoKey);
            return cryptoKey;
        }

    }
}
