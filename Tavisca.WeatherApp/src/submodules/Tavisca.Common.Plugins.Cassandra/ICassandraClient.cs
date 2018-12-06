using Cassandra;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Cassandra
{
    public interface ICassandraClient
    {
        Task<RowSet> ExecuteAsync(IStatement statement);

        Task<PreparedStatement> PrepareStatementAsync(string statement);

        Task<string> GetMetadataAsync();
        void Dispose();
    }
}
