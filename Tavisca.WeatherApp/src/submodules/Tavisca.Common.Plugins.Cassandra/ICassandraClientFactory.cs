using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Cassandra
{
    public interface ICassandraClientFactory
    {
        Task<ICassandraClient> GetClientAsync();
    }
}
