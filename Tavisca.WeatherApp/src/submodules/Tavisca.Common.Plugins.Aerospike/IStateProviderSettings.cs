using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Aerospike
{
    public interface IStateProviderSettings
    {
        Task<AerospikeSettings> GetSettings();
    }
}
