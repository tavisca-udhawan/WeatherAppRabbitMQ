using System.Threading.Tasks;

namespace Tavisca.Platform.Common.FileStore
{
    public interface IFileStoreFactory
    {
        IFileStore GetTenantSpecificClient(string tenantId);
        Task<IFileStore> GetTenantSpecificClientAsync(string tenantId);
        IFileStore GetGlobalClient();
        Task<IFileStore> GetGlobalClientAsync();
    }
}
