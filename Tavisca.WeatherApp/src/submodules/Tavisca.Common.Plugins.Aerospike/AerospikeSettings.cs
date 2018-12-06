using System.Collections.Generic;

namespace Tavisca.Common.Plugins.Aerospike
{
    public class AerospikeSettings
    {
        public string Host { get; set; }
        public List<string> SecondaryHosts { get; } = new List<string>();
        public int Port { get; set; }
        public string Namespace { get; set; }
        public string SecondaryNamespace { get; set; }
        public string Set { get; set; }
    }
}
