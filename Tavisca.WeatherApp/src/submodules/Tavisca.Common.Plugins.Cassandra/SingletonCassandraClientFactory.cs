using Cassandra;
using System;
using System.Threading.Tasks;
using Tavisca.Platform.Common;
using Tavisca.Platform.Common.ApplicationEventBus;
using CompressionType = Cassandra.CompressionType;

namespace Tavisca.Common.Plugins.Cassandra
{
    public class SingletonCassandraClientFactory : ApplicationEventObserver, ICassandraClientFactory
    {
        private static readonly AsyncLock _lock = new AsyncLock();
        private static ICassandraClient _client;
        private static string _signature = string.Empty;
        private static ICassandraSettingsProvider _configProvider;

        public SingletonCassandraClientFactory(ICassandraSettingsProvider config)
        {
            var applicationBus = ObjectFactory.GetInstance<IApplicationEventBus>();
            applicationBus.Register("in-memory-consul-cache-refresh", this);

            _configProvider = config;
        }
        public async Task<ICassandraClient> GetClientAsync()
        {
            return await GetClient();
        }

        private ISession CreateSession(CassandraSettings db)
        {
            CompressionType compression = CompressionType.LZ4;
            if (string.IsNullOrWhiteSpace(db.Compression))
                compression = CompressionType.LZ4;
            else
            {
                if (Enum.TryParse(db.Compression, out compression) == false)
                    compression = CompressionType.LZ4;
            }

            try
            {
                return string.IsNullOrEmpty(db.Username) || string.IsNullOrEmpty(db.Password)
                    ? Cluster
                    .Builder()
                    .AddContactPoints(db.Hosts)
                    .Build()
                    .Connect(db.KeySpace)
                    : Cluster
                    .Builder()
                    .AddContactPoints(db.Hosts)
                    .WithCredentials(db.Username, db.Password)
                    .Build()
                    .Connect(db.KeySpace);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<ICassandraClient> GetClient()
        {
            if (_client != null)
                return _client;
            else
            {
                using (await _lock.LockAsync())
                {
                    if (_client != null)
                        return _client;

                    CassandraSettings settings = await _configProvider.GetSettings();
                    _client = CreateClient(settings);
                    _signature = settings.Signature;
                    return _client;
                }
            }
        }

        private ICassandraClient CreateClient(CassandraSettings settings)
        {
            var cassandraClient = new CassandraClient(CreateSession(settings))
            {
                RetrySetting = settings.RetrySetting ?? new RetrySetting()
            };
            return cassandraClient;
        }

        public override async void Process(ApplicationEvent msg)
        {
            using (await _lock.LockAsync())
            {
                CassandraSettings updatedSettings = await _configProvider.GetSettings();
                if (_signature.Equals(updatedSettings.Signature) == false)
                {
                    _client = CreateClient(updatedSettings);
                    _signature = updatedSettings.Signature;
                }
            }
        }
    }

}