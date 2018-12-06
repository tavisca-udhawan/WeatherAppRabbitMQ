using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.SessionStore;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.MemoryStreamPool;
using Tavisca.Platform.Common.Serialization;

namespace Tavisca.Common.Plugins.Aerospike
{
    public class AerospikeSessionProviderFactory : ISessionProviderFactory
    {
        private IConfigurationProvider _configurationProvider;
        private IAerospikeClientFactory _clientFactory;
        private ISerializerFactory _serializerFactory;
        private IMemoryStreamPool _memoryStreamPool;

        public AerospikeSessionProviderFactory(IAerospikeClientFactory clientFactory, IConfigurationProvider configurationProvider, ISerializerFactory serializerFactory, IMemoryStreamPool memoryStreamPool)
        {
            _clientFactory = clientFactory;
            _configurationProvider = configurationProvider;
            _serializerFactory = serializerFactory;
            _memoryStreamPool = memoryStreamPool;
        }
        public ISessionDataProvider GetSessionProvider()
        {
            return new AeroSpikeSessionProvider(_clientFactory, _configurationProvider, _serializerFactory, _memoryStreamPool);
        }
    }
}
