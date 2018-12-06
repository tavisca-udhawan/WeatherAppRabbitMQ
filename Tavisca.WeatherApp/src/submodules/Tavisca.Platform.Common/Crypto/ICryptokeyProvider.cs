using System.Security;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Crypto
{
    public interface ICryptoKeyProvider
    {
        SecureString GenerateCryptoKey(string application, string tenantId, string keyIdentifier);
        Task<SecureString> GenerateCryptoKeyAsync(string application, string tenantId, string keyIdentifier);
        SecureString GetCryptoKey(string application, string tenantId, string keyIdentifier);
        Task<SecureString> GetCryptoKeyAsync(string application, string tenantId, string keyIdentifier);
    }
}
