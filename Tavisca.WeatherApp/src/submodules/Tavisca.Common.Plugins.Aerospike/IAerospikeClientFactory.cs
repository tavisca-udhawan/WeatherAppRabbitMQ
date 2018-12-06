using Aerospike.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Aerospike
{
    public interface IAerospikeClientFactory
    {
        [System.Obsolete("This has been marked obsolete because we have introduced support for secondary hosts")]
        AsyncClient GetClient(string host, int port);
        AsyncClient GetClient(string host, int port, List<string> secondaryHosts);
    }
}
