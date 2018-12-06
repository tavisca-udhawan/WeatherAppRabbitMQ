using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Aerospike
{
    public interface ILockProviderSettings
    {
        Task<AerospikeSettings> GetSettings();
    }
}
