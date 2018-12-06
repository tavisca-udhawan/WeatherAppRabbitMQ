using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Crypto
{
    public interface ICryptoService
    {
        string Encrypt(string application, string tenantId, string keyIdentifer, string plainText);
        Task<string> EncryptAsync(string application, string tenantId, string keyIdentifier, string plainText);
        Task<byte[]> EncryptAsync(string application, string tenantId, string keyIdentifier, byte[] plainTextBytes);

        string Decrypt(string application, string tenantId, string keyIdentifier, string cipherText);
        Task<string> DecryptAsync(string application, string tenantId, string keyIdentifier, string cipherText);
        Task<byte[]> DecryptAsync(string application, string tenantId, string keyIdentifier, byte[] cipherTextBytes);
    }
}
