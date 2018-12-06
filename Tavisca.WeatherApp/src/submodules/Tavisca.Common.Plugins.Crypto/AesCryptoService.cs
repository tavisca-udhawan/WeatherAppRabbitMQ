using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Crypto;
using Tavisca.Platform.Common.Profiling;

namespace Tavisca.Common.Plugins.Crypto
{
    public class AesCryptoService : ICryptoService
    {
        private readonly ICryptoKeyProviderFactory _keyProviderFactory;
        public AesCryptoService(ICryptoKeyProviderFactory keyProviderFactory)
        {
            _keyProviderFactory = keyProviderFactory;
        }

        public async Task<string> EncryptAsync(string application, string tenantId, string keyIdentifier, string plainText)
        {
            using (var scope = new ProfileContext("AesCryptoService: encrypt_async"))
            {
                Validators.ValidateEncryptRequest(application, tenantId, keyIdentifier, plainText);
                var client = await _keyProviderFactory.GetKeyProviderAsync();
                var dataKey = await client.GenerateCryptoKeyAsync(application, tenantId, keyIdentifier);
                var cipherText = Encrypt(dataKey, GetBytes(plainText));
                
                //encrypted data is in Base64 encoded returned by AWS
                return Convert.ToBase64String(cipherText);
            }
        }

        public async Task<byte[]> EncryptAsync(string application, string tenantId, string keyIdentifier, byte[] plainTextBytes)
        {
            using (var scope = new ProfileContext("AesCryptoService: encrypt_async"))
            {
                Validators.ValidateEncryptRequest(application, tenantId, keyIdentifier, plainTextBytes);
                var client = await _keyProviderFactory.GetKeyProviderAsync();
                var dataKey = await client.GenerateCryptoKeyAsync(application, tenantId, keyIdentifier);
                return Encrypt(dataKey, plainTextBytes);
            }
        }

        public async Task<string> DecryptAsync(string application, string tenantId, string keyIdentifier, string cipherText)
        {
            using (var scope = new ProfileContext("AesCryptoService: decrypt_async"))
            {
                Validators.ValidateDecryptRequest(application, tenantId, keyIdentifier, cipherText);
                var client = await _keyProviderFactory.GetKeyProviderAsync();
                var dataKey = await client.GetCryptoKeyAsync(application, tenantId, keyIdentifier);
                //cipherText in is Base64 encoded
                var plainText = Decrypt(dataKey, Convert.FromBase64String(cipherText));

                return GetStringFromBytes(plainText);
            }
        }

        public async Task<byte[]> DecryptAsync(string application, string tenantId, string keyIdentifier, byte[] cipherTextBytes)
        {
            using (var scope = new ProfileContext("AesCryptoService: decrypt_async"))
            {
                Validators.ValidateDecryptRequest(application, tenantId, keyIdentifier, cipherTextBytes);
                var client = await _keyProviderFactory.GetKeyProviderAsync();
                var dataKey = await client.GetCryptoKeyAsync(application, tenantId, keyIdentifier);
                return Decrypt(dataKey, cipherTextBytes);
            }
        }

        public string Encrypt(string application, string tenantId, string keyIdentifier, string plainText)
        {
            using (var scope = new ProfileContext("AesCryptoService: encrypt_sync"))
            {
                Validators.ValidateEncryptRequest(application, tenantId, keyIdentifier, plainText);
                var client = _keyProviderFactory.GetKeyProvider();
                var dataKey = client.GenerateCryptoKey(application, tenantId, keyIdentifier);
                var cipherText = Encrypt(dataKey, GetBytes(plainText));
                
                //encrypted data is in Base64 encoded returned by AWS
                return Convert.ToBase64String(cipherText);
            }
        }

        public string Decrypt(string application, string tenantId, string keyIdentifier, string cipherText)
        {
            using (var scope = new ProfileContext("AesCryptoService: decrypt_sync"))
            {
                Validators.ValidateDecryptRequest(application, tenantId, keyIdentifier, cipherText);
                var client = _keyProviderFactory.GetKeyProvider();
                var dataKey = client.GetCryptoKey(application, tenantId, keyIdentifier);
                var plainText = Decrypt(dataKey, Convert.FromBase64String(cipherText));
                
                return GetStringFromBytes(plainText);
            }
        }

        //Encrypt using AES_256
        private byte[] Encrypt(SecureString encryptionKey, byte[] plainTextBytes)
        {
            using (var scope = new ProfileContext("AesCryptoService: encrypt data using AES"))
            {
                var encryptionKeyInBytes = ConvertSecureStrToBytes(encryptionKey);
                //encrypt input using key and AES_256 algorithm
                var encryptedText = string.Empty;

                byte[] encrypted;
                byte[] IV;

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = encryptionKeyInBytes;

                    aesAlg.GenerateIV();
                    IV = aesAlg.IV;

                    aesAlg.Mode = CipherMode.CBC;

                    var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for encryption. 
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            csEncrypt.Write(plainTextBytes, 0, plainTextBytes.Length);
                            csEncrypt.FlushFinalBlock();
                             
                            encrypted = msEncrypt.ToArray();
                        }
                    }
                }

                var combinedIvCt = new byte[IV.Length + encrypted.Length];
                Array.Copy(IV, 0, combinedIvCt, 0, IV.Length);
                Array.Copy(encrypted, 0, combinedIvCt, IV.Length, encrypted.Length);

                Array.Clear(encryptionKeyInBytes, 0, encryptionKeyInBytes.Length);
                // Return the encrypted bytes from the memory stream. 
                return combinedIvCt;
            }
        }

        //Decrypt using AES_256
        private byte[] Decrypt(SecureString decryptionKey, byte[] cipherTextBytes)
        {
            using (var scope = new ProfileContext("AesCryptoService: decrypt data using AES"))
            {
                var decryptionKeyBytes = ConvertSecureStrToBytes(decryptionKey);

                // Declare the string used to hold the decrypted text. 
                byte[] plainTextBytes;

                // Create an Aes object with the specified key and IV. 
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = decryptionKeyBytes;

                    byte[] IV = new byte[aesAlg.BlockSize / 8];
                    byte[] bytesToBedecrypted = new byte[cipherTextBytes.Length - IV.Length];

                    Array.Copy(cipherTextBytes, IV, IV.Length);
                    Array.Copy(cipherTextBytes, IV.Length, bytesToBedecrypted, 0, bytesToBedecrypted.Length);

                    aesAlg.IV = IV;
                    aesAlg.Mode = CipherMode.CBC;

                    // Create a decrytor to perform the stream transform.
                    var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    // Create the streams used for decryption.
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(bytesToBedecrypted, 0, bytesToBedecrypted.Length);

                            //cryptoStream.FlushFinalBlock();
                        }

                        plainTextBytes = memoryStream.ToArray();
                    } 
                }

                Array.Clear(decryptionKeyBytes, 0, decryptionKeyBytes.Length);
                return plainTextBytes;
            }
        }

        private byte[] ConvertSecureStrToBytes(SecureString secureKey)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
#if !NET_STANDARD
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureKey);
#else
                unmanagedString = SecureStringMarshal.SecureStringToGlobalAllocUnicode(secureKey);
#endif
                return Convert.FromBase64String(Marshal.PtrToStringUni(unmanagedString));
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        private byte[] GetBytes(string text)
        {
           return Encoding.UTF8.GetBytes(text);
        }

        private string GetStringFromBytes(byte[] value)
        {
            return Encoding.UTF8.GetString(value);
        }
    }
}
