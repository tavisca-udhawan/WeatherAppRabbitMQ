using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Spooling
{
    internal class SpoolerKeyCollection
    {
        public SpoolerKeyCollection(string txnId, int count)
        {
            _statusKeys = new string[count];
            _resultKeys = new string[count];
            for (int i = 0; i < count; i++)
            {
                _statusKeys[i] = BuildStatusKey(txnId, i);
                _resultKeys[i] = BuildResultKey(txnId, i);
                _reverseMapping[_statusKeys[i]] = i;
                _reverseMapping[_resultKeys[i]] = i;
            }
        }

        private string[] _statusKeys { get; }

        private string[] _resultKeys { get; }

        public IEnumerable<string> StatusKeys
        {
            get { return _statusKeys; }
        }

        public string GetStatusKey(int index)
        {
            return _statusKeys[index];
        }

        public string GetResultKey(int index)
        {
            return _resultKeys[index];
        }

        public int ResolveIndex(string key)
        {
            return _reverseMapping[key];
        }

        private Dictionary<string, int> _reverseMapping = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        private static string BuildStatusKey(string txnId, int index)
        {
            return $"Status_{txnId}_{index}";
        }

        private static string BuildResultKey(string txnId, int index)
        {
            return $"Result_{txnId}_{index}";
        }

    }
}

/*
Specification:
- Gets a delegate to invoke along with the input arg.
- Delegate will either run or fault.

- Should be able to create a spooler with an existing txn id.
    var spooler = new Spooler(txnId);
    
- Should get results based on a previous continuation token.
    var results = spooler.GetResults(bookmark);

*/
