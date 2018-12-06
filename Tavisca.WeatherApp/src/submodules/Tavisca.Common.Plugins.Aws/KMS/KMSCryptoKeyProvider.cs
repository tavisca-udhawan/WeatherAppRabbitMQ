using System.Threading.Tasks;
using System.IO;
using System;
using System.Collections.Generic;
using System.Security;
using System.Net;
using System.Linq;
using Tavisca.Platform.Common.Crypto;
using Amazon.KeyManagementService.Model;
using Tavisca.Platform.Common.LockManagement;
using Tavisca.Platform.Common.Context;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Profiling;
using Tavisca.Platform.Common.Configurations;
using System.Threading;

namespace Tavisca.Common.Plugins.Aws.KMS
{
    public class KmsCryptoKeyProvider : ICryptoKeyProvider
    {
        private readonly IConfigurationProvider _config;
        private readonly IKmsClientFactory _kmsClientFactory;
        private readonly ICryptoKeyStore _cryptoKeyStore;
        private GlobalLock _globalLock;
        private static readonly string _lockKey = "{0}/{1}/{2}";
        private readonly ILockProvider _lockProvider;
        private static readonly string _keySpec = "AES_256";

        public KmsCryptoKeyProvider(IConfigurationProvider config, IKmsClientFactory kmsClientFactory, ICryptoKeyStore cryptoKeyStore, ILockProvider lockProvider)
        {
            _config = config;
            _cryptoKeyStore = cryptoKeyStore;
            _lockProvider = lockProvider;
            _kmsClientFactory = kmsClientFactory;
            _globalLock = new GlobalLock(lockProvider);
        }

        public async Task<SecureString> GenerateCryptoKeyAsync(string application, string tenantId, string keyIdentifier)
        {
            using (var scope = new ProfileContext("KmsCryptoKeyProvider : Generate crypto key"))
            {
                var cryptoKeyInBytes = await _cryptoKeyStore.GetAsync(application, tenantId, keyIdentifier);
                if (cryptoKeyInBytes == null)
                {
                    var lockKey = GenerateLockKey(application, tenantId, keyIdentifier);

                    using (await _globalLock.EnterWriteLock(lockKey))
                    {
                        cryptoKeyInBytes = await _cryptoKeyStore.GetAsync(application, tenantId, keyIdentifier);
                        if (cryptoKeyInBytes == null)
                        {
                            return await GetDataKeyFromKmsAsync(application, tenantId, keyIdentifier);
                        }
                    }
                }

                return await DecryptCryptoKeyAsync(application, tenantId, cryptoKeyInBytes);
            }
        }

        public async Task<SecureString> GetCryptoKeyAsync(string application, string tenantId, string keyIdentifier)
        {
            using (var scope = new ProfileContext("KmsCryptoKeyProvider : Get crypto key"))
            {
                var cryptoKeyInBytes = await _cryptoKeyStore.GetAsync(application, tenantId, keyIdentifier);

                if (cryptoKeyInBytes == null)
                    throw Errors.ServerSide.CryptoKeyNotFound();

                return await DecryptCryptoKeyAsync(application, tenantId, cryptoKeyInBytes);
            }
        }

        public SecureString GenerateCryptoKey(string application, string tenantId, string keyIdentifier)
        {
            using (var scope = new ProfileContext("KmsCryptoKeyProvider : Generate crypto key"))
            {
                var cryptoKeyInBytes = _cryptoKeyStore.Get(application, tenantId, keyIdentifier);
                if (cryptoKeyInBytes == null)
                {
                    var lockKey = GenerateLockKey(application, tenantId, keyIdentifier);

                    using (EnterGlobalWriteLock(lockKey))
                    {
                        cryptoKeyInBytes = _cryptoKeyStore.Get(application, tenantId, keyIdentifier);
                        if (cryptoKeyInBytes == null)
                        {
                            return GetDataKeyFromKms(application, tenantId, keyIdentifier);
                        }
                    }
                }

                return DecryptCryptoKey(application, tenantId, cryptoKeyInBytes);
            }
        }

        public SecureString GetCryptoKey(string application, string tenantId, string keyIdentifier)
        {
            using (var scope = new ProfileContext("KmsCryptoKeyProvider : Get crypto key"))
            {
                var cryptoKeyInBytes = _cryptoKeyStore.Get(application, tenantId, keyIdentifier);

                if (cryptoKeyInBytes == null)
                    throw Errors.ServerSide.CryptoKeyNotFound();

                return DecryptCryptoKey(application, tenantId, cryptoKeyInBytes);
            }
        }

        private static string GenerateLockKey(string application, string tenantId, string keyIdentifier)
        {
            return string.Format(_lockKey, application, tenantId, keyIdentifier);
        }

        private async Task<SecureString> GetDataKeyFromKmsAsync(string application, string tenantId, string keyIdentifier)
        {
            using (var scope = new ProfileContext($"Generating new crypto key using kms for {application} {tenantId}"))
            {
                GenerateDataKeyRequest request = null;
                GenerateDataKeyResponse response = null;
                bool isSuccess = false;
                try
                {
                    var client = await _kmsClientFactory.GetGlobalClientAsync();
                    request = await GetGenerateDataKeyRequestAsync(application, tenantId, keyIdentifier);
                    response = await client.GenerateDataKeyAsync(request);
                    if (response == null || response.HttpStatusCode != HttpStatusCode.OK)
                        throw Errors.ServerSide.KMSCommunicationError();

                    //CiperTextBlob is Base64-encoded binary data 
                    await _cryptoKeyStore.AddAsync(application, tenantId, keyIdentifier, response.CiphertextBlob.ToArray());
                    isSuccess = true;
                    //PlaintText is Base64-encoded binary data 
                    return ConvertStreamToSecureString(response.Plaintext);

                }
                catch (Exception ex)
                {
                    Platform.Common.ExceptionPolicy.HandleException(ex, Constants.LogOnlyPolicy);
                }
                finally
                {
                    await LogRQRS(request, SanitizedResponse(response), application, tenantId, "aws_kms_provider", "generate_datakey", isSuccess);
                }
                throw Errors.ServerSide.KMSCommunicationError();
            }
        }

        private SecureString GetDataKeyFromKms(string application, string tenantId, string keyIdentifier)
        {
            using (var scope = new ProfileContext($"Generating new crypto key using kms for {application} {tenantId}"))
            {
                GenerateDataKeyRequest request = null;
                GenerateDataKeyResponse response = null;
                bool isSuccess = false;
                try
                {
                    var client = _kmsClientFactory.GetGlobalClient();
                    request = GetGenerateDataKeyRequest(application, tenantId, keyIdentifier);
                    var waitHandle = new ManualResetEvent(false);

                    Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            response = await client.GenerateDataKeyAsync(request);
                        }
                        catch (Exception ex)
                        {
                            Platform.Common.ExceptionPolicy.HandleException(ex, Constants.LogOnlyPolicy);
                            throw Errors.ServerSide.KMSCommunicationError();
                        }
                        finally
                        {
                            waitHandle.Set();
                        }
                    });
                    waitHandle.WaitOne();
                    if (response == null || response.HttpStatusCode != HttpStatusCode.OK)
                        throw Errors.ServerSide.KMSCommunicationError();

                    //CiperTextBlob is Base64-encoded binary data 
                    _cryptoKeyStore.Add(application, tenantId, keyIdentifier, response.CiphertextBlob.ToArray());
                    isSuccess = true;
                    //PlaintText is Base64-encoded binary data 
                    return ConvertStreamToSecureString(response.Plaintext);
                }
                finally
                {
                    LogRQRS(request, SanitizedResponse(response), application, tenantId, "aws_kms_provider", "generate_datakey", isSuccess);
                }
            }
        }

        private async Task<SecureString> DecryptCryptoKeyAsync(string application, string tenantId, byte[] cipherTextBytes)
        {
            using (var scope = new ProfileContext($"Decrypting crypto key using kms for {application} {tenantId}"))
            {
                DecryptRequest request = null;
                DecryptResponse response = null;
                bool isSuccess = false;
                try
                {
                    request = GetDecryptRequest(application, tenantId, cipherTextBytes);

                    var client = await _kmsClientFactory.GetGlobalClientAsync();

                    response = await client.DecryptAsync(request);
                    if (response == null || response.HttpStatusCode != HttpStatusCode.OK)
                        throw Errors.ServerSide.KMSCommunicationError();
                    isSuccess = true;
                    //PlaintText is Base64-encoded binary data 
                    return ConvertStreamToSecureString(response.Plaintext);

                }
                catch (Exception ex)
                {
                    Platform.Common.ExceptionPolicy.HandleException(ex, Constants.LogOnlyPolicy);
                }
                finally
                {
                    request.CiphertextBlob = null;
                    await LogRQRS(request, SanitizedResponse(response), application, tenantId, "aws_kms_provider", "decrypt_key", isSuccess);
                }
                throw Errors.ServerSide.KMSCommunicationError();
            }
        }

        private SecureString DecryptCryptoKey(string application, string tenantId, byte[] cipherTextBytes)
        {
            using (var scope = new ProfileContext($"Decrypting crypto key using kms for {application} {tenantId}"))
            {
                DecryptRequest request = null;
                DecryptResponse response = null;
                bool isSuccess = false;
                try
                {
                    request = GetDecryptRequest(application, tenantId, cipherTextBytes);

                    var client = _kmsClientFactory.GetGlobalClient();

                    var waitHandle = new ManualResetEvent(false);

                    Task.Factory.StartNew(async () =>
                    {
                        try
                        {
                            response = await client.DecryptAsync(request);
                        }
                        catch (Exception ex)
                        {
                            Platform.Common.ExceptionPolicy.HandleException(ex, Constants.LogOnlyPolicy);
                            throw Errors.ServerSide.KMSCommunicationError();
                        }
                        finally
                        {
                            waitHandle.Set();
                        }
                    });
                    waitHandle.WaitOne();

                    if (response == null || response.HttpStatusCode != HttpStatusCode.OK)
                        throw Errors.ServerSide.KMSCommunicationError();
                    isSuccess = true;
                    //PlaintText is Base64-encoded binary data 
                    return ConvertStreamToSecureString(response.Plaintext);
                }
                finally
                {
                    request.CiphertextBlob = null;
                    LogRQRS(request, SanitizedResponse(response), application, tenantId, "aws_kms_provider", "decrypt_key", isSuccess);
                }
            }
        }

        private async Task<GenerateDataKeyRequest> GetGenerateDataKeyRequestAsync(string application, string tenantId, string keyId)
        {
            return new GenerateDataKeyRequest
            {
                KeyId = string.Format(Constants.KeyId, keyId),
                KeySpec = await GetKeySpecAsync(tenantId),
                EncryptionContext = GetEncryptionContext(application, tenantId)
            };
        }

        private GenerateDataKeyRequest GetGenerateDataKeyRequest(string application, string tenantId, string keyId)
        {
            return new GenerateDataKeyRequest
            {
                KeyId = string.Format(Constants.KeyId, keyId),
                KeySpec = GetKeySpec(tenantId),
                EncryptionContext = GetEncryptionContext(application, tenantId)
            };
        }

        private async Task<string> GetKeySpecAsync(string tenantId)
        {
            return await _config.GetTenantConfigurationAsStringAsync(tenantId, Constants.DataEncryptionSection, Constants.KeySpec) ?? _keySpec;
        }

        private string GetKeySpec(string tenantId)
        {
            return _config.GetTenantConfigurationAsString(tenantId, Constants.DataEncryptionSection, Constants.KeySpec) ?? _keySpec;
        }

        private Dictionary<string, string> GetEncryptionContext(string application, string tenantId)
        {
            var context = new Dictionary<string, string>();
            context.Add("application", application);
            context.Add("tenandId", tenantId);

            return context;
        }

        private SecureString ConvertStreamToSecureString(MemoryStream plainTextBlob)
        {
            var plainText = Convert.ToBase64String(plainTextBlob.ToArray());

            SecureString secPlainText = new SecureString();
            plainText.ToCharArray().ToList().ForEach((q) => secPlainText.AppendChar(q));

            plainText = null;
            Clear(plainTextBlob);

            return secPlainText;
        }

        public static void Clear(MemoryStream source)
        {
            source.Position = 0;
            source.SetLength(0);
        }

        private DecryptResponse SanitizedResponse(DecryptResponse response)
        {
            if (response == null)
                return response;

            response.Plaintext = null;
            response.ResponseMetadata = null;
            return response;
        }

        private GenerateDataKeyResponse SanitizedResponse(GenerateDataKeyResponse response)
        {
            if (response == null)
                return response;

            response.CiphertextBlob = null;
            response.Plaintext = null;
            response.ResponseMetadata = null;
            return response;
        }

        private async Task LogRQRS(object request, object response, string application, string tenantId, string api, string verb, bool isSuccess)
        {
            var context = CallContext.Current;

            var log = new ApiLog
            {
                ApplicationName = application,
                Verb = verb,
                Request = new Payload(Platform.Common.Plugins.Json.ByteHelper.ToByteArrayUsingJsonSerialization(request)),
                Response = new Payload(Platform.Common.Plugins.Json.ByteHelper.ToByteArrayUsingJsonSerialization(response)),
                ApplicationTransactionId = context?.TransactionId,
                CorrelationId = context?.CorrelationId,
                StackId = context?.StackId,
                TenantId = context?.TenantId,
                Api = api,
                IsSuccessful = isSuccess
            };

            await Logger.WriteLogAsync(log);
        }

        private DecryptRequest GetDecryptRequest(string application, string tenantId, byte[] cipherTextBytes)
        {
            return new DecryptRequest
            {
                CiphertextBlob = new MemoryStream(cipherTextBytes),
                EncryptionContext = GetEncryptionContext(application, tenantId)
            };
        }

        private IDisposable EnterGlobalWriteLock(string lockKey)
        {
            var waitHandle = new ManualResetEvent(false);

            IDisposable padLock = null;

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    padLock = await _globalLock.EnterWriteLock(lockKey);
                }
                finally
                {
                    waitHandle.Set();
                }
            });
            waitHandle.WaitOne();

            return padLock;
        }
    }
}
