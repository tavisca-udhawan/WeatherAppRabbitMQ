using System.Threading.Tasks;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.FileStore;
using Tavisca.Platform.Common.Profiling;

namespace Tavisca.Common.Plugins.Aws.KMS
{
    public class CryptoKeyStore : ICryptoKeyStore
    {
        private readonly IConfigurationProvider _config;
        private readonly IFileStoreFactory _storeFactory;
        public CryptoKeyStore(IConfigurationProvider config, IFileStoreFactory storeFactory)
        {
            _config = config;
            _storeFactory = storeFactory;
        }

        public async Task AddAsync(string application, string tenantId, string keyIdentifier, byte[] value)
        {
            using (var scope = new ProfileContext($"Add key to crypto key store: application {application} and tenant {tenantId}"))
            {
                var client = await _storeFactory.GetGlobalClientAsync();
                string formattedKey = ConstructKey(application, tenantId, keyIdentifier);
                string bucket = await GetBucketAsync();
                await client.AddAsync(formattedKey, bucket, value);
            }
        }

        public async Task<byte[]> GetAsync(string application, string tenantId, string keyIdentifier)
        {
            using (var scope = new ProfileContext($"Get key from crypto key store : application {application} and tenant {tenantId}"))
            {
                var client = await _storeFactory.GetGlobalClientAsync();
                string formattedKey = ConstructKey(application, tenantId, keyIdentifier);
                string bucket = await GetBucketAsync();
                return await client.GetAsync(formattedKey, bucket);
            }
        }

        public void Add(string application, string tenantId, string keyIdentifier, byte[] value)
        {
            using (var scope = new ProfileContext($"Add key to crypto key store: application {application} and tenant {tenantId}"))
            {
                var client =  _storeFactory.GetGlobalClient();
                string formattedKey = ConstructKey(application, tenantId, keyIdentifier);
                string bucket = GetBucket();
                client.Add(formattedKey, bucket, value);
            }
        }

        public byte[] Get(string application, string tenantId, string keyIdentifier)
        {
            using (var scope = new ProfileContext($"Get key from crypto key store : application {application} and tenant {tenantId}"))
            {
                var client = _storeFactory.GetGlobalClient();
                string formattedKey = ConstructKey(application, tenantId, keyIdentifier);
                string bucket = GetBucket();
                return client.Get(formattedKey, bucket);
            }
        }
        
        private async Task<string> GetBucketAsync()
        {
            var bucketName = await _config.GetGlobalConfigurationAsStringAsync(Constants.DataEncryptionSection, Constants.BucketName);
            if (string.IsNullOrWhiteSpace(bucketName))
                throw Errors.ServerSide.S3BucketMissingConfiguration();

            return bucketName;
        }

        private string GetBucket()
        {
            var bucketName = _config.GetGlobalConfigurationAsString(Constants.DataEncryptionSection, Constants.BucketName);
            if (string.IsNullOrWhiteSpace(bucketName))
                throw Errors.ServerSide.S3BucketMissingConfiguration();

            return bucketName;

        }

        private static string ConstructKey(string application, string tenantId, string keyIdentifier)
        {
            return string.Format("{0}/{1}/{2}", application, tenantId, keyIdentifier);
        }
    }
}

