using Cassandra;
using System;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common;
using Tavisca.Platform.Common.Profiling;

namespace Tavisca.Common.Plugins.Cassandra
{
    public class CassandraClient : ICassandraClient
    {
        private ISession _session;

        public CassandraClient(ISession session)
        {
            _session = session;
        }

        private readonly StatementCache _cache = new StatementCache();

        public RetrySetting RetrySetting { get; set; }


        public async Task<RowSet> ExecuteAsync(IStatement statement)
        {
            return await ExecuteStatement(statement);
        }

        private async Task<RowSet> ExecuteStatement(IStatement statement)
        {
            using (new TraceProfileContext("ExecuteQuery", "Execute query"))
            {
                RowSet rowSet = null;
                var numberOfAttempt = 0;
                do
                {
                    try
                    {
                        rowSet =  await _session.ExecuteAsync(statement);
                        return rowSet;
                    }
                    catch (NoHostAvailableException noHostAvailableEx)
                    {
                        ExceptionPolicy.HandleException(noHostAvailableEx, "logonly");
                        await Task.Delay(RetrySetting.DelayInMs);
                        numberOfAttempt++;
                        Profiling.Trace("Number of attempt(s) " + (numberOfAttempt).ToString());
                        if (numberOfAttempt > RetrySetting.NumberOfRetry)
                            throw noHostAvailableEx;                        
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                } while (numberOfAttempt <= RetrySetting.NumberOfRetry);

                return rowSet; 
            }
        }

        public async Task<PreparedStatement> PrepareStatementAsync(string cql)
        {
            if (_cache.TryGetStatement(cql, out PreparedStatement statement) == true)
            {
                return statement;
            }

            statement = await _session.PrepareAsync(cql);
            _cache.Add(cql, statement);
            return statement;
        }

        public async Task<string> GetMetadataAsync()
        {
            var metadata = _session.Cluster.Metadata;
            var sb = new StringBuilder();
            sb.Append($" cluster name : {metadata.ClusterName}");
            var hosts = metadata.AllHosts();
            if (hosts?.Count > 0)
            {
                int i = 1;
                foreach (var host in hosts)
                {
                    sb.Append($" | host {i}");
                    sb.Append($" -> address : {host.Address}");
                    sb.Append($" -> isUp: {host.IsUp}");
                    sb.Append($" -> isConsiderablyUp: {host.IsConsiderablyUp}");
                }
                i++;
            }
            return await Task.FromResult(sb.ToString());
        }

        public void Dispose()
        {
            _session.Dispose();
        }
    }
}
