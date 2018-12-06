using Amazon.KeyManagementService;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Aws.KMS
{
    public interface IKmsClientFactory
    {
        Task<IAmazonKeyManagementService> GetClientAsync(KmsSettings settings);
        Task<IAmazonKeyManagementService> GetTenantSpecificClientAsync(string tenantId);
        Task<IAmazonKeyManagementService> GetGlobalClientAsync();

        IAmazonKeyManagementService GetClient(KmsSettings settings);
        IAmazonKeyManagementService GetTenantSpecificClient(string tenantId);
        IAmazonKeyManagementService GetGlobalClient();
    }
}
