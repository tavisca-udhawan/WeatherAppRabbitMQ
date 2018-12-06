using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Aerospike
{
    public interface ICounterSettings
    {
        Task<AerospikeSettings> GetSettings();
    }
}
