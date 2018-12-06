using System.IO;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Aws
{
    public interface ICryptoKeyStore
    {
        Task AddAsync(string application, string tenant, string keyIdentifier, byte[] bytes);
        Task<byte[]> GetAsync(string application, string tenant, string keyIdentifier);

        void Add(string application, string tenant, string keyIdentifier, byte[] bytes);
        byte[] Get(string application, string tenant, string keyIdentifier);
    }
}
