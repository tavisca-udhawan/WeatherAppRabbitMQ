using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Cassandra
{
    public interface ICassandraSettingsProvider
    {
        Task<CassandraSettings> GetSettings();
    }
}
