using RabbitMQ.Client;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.RabbitMq.Configuration;

namespace Tavisca.Common.Plugins.RabbitMq
{
    internal static class ChannelFactory
    {
        private static ConcurrentDictionary<Endpoint, IConnection> _connectionCache =
            new ConcurrentDictionary<Endpoint, IConnection>();

        public static async Task<IModel> CreateChannel(Endpoint endpoint)
        {
            var connection = _connectionCache.GetOrAdd(endpoint, CreateConnection);
            IModel channel = null;
            do
            {
                try
                {
                    channel = await TryCreateChannel(connection);
                }
                catch (Exception)
                {
                    //ignore exception 
                }
            } while (channel == null);

            channel.BasicQos(0, 50, false);

            return channel;
        }

        private static async Task<IModel> TryCreateChannel(IConnection connection)
        {
            do
            {
                if (connection.IsOpen)
                    break;

                await Task.Delay(TimeSpan.FromSeconds(5));
            } while (true);

            return connection.CreateModel();
        }

        private static IConnection CreateConnection(Endpoint endpoint)
        {
            var connectionFactory = new ConnectionFactory()
            {
                HostName = endpoint.HostName,
                VirtualHost = endpoint.VirtualHost,
                UserName = endpoint.UserName,
                Password = endpoint.Password,
                Port = endpoint.Port,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };

            return connectionFactory.CreateConnection();
        }
    }
}
