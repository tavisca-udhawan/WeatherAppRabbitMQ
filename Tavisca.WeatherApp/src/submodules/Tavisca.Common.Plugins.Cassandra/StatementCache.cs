using Cassandra;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Tavisca.Common.Plugins.Cassandra
{
    internal class StatementCache
    {
        private ConcurrentDictionary<string, PreparedStatement> _cache = new ConcurrentDictionary<string, PreparedStatement>(StringComparer.OrdinalIgnoreCase);

        public bool TryGetStatement(string cql, out PreparedStatement statement)
        {
            return _cache.TryGetValue(cql, out statement);
        }

        public void Add(string cql, PreparedStatement statement)
        {
            _cache[cql] = statement;
        }
    }
}
