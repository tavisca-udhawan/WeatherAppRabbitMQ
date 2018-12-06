using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.MemoryStreamPool;
using Tavisca.Platform.Common.Serialization;
using Tavisca.Platform.Common.ServiceBus;



namespace Tavisca.Common.Plugins.RabbitMq
{
    public class RmqBus : IBus
    {
        internal ISerializer Serializer { get; }
        internal IQueueSettings Settings { get; }
        internal ConnectionPool Connections { get; }
        internal RmqSubscriptions Subscriptions { get; }
        internal IMemoryStreamPool MemoryStreamPool { get; }

        public RmqBus(IQueueSettings settings, ISerializer serializer, IMemoryStreamPool memoryStreamPool)
        {
            Settings = settings;
            Serializer = serializer;
            MemoryStreamPool = memoryStreamPool;
            Connections = new ConnectionPool();
            Subscriptions = new RmqSubscriptions(this);
        }

        public async Task PublishAsync<T>(string topic, T message)
        {
            var body = await Serialize(message);
            var queue = await Settings.GetWriteQueueAsync(topic);
            using (var channel = Connections.CreateChannel(queue))
            {
                channel.BasicPublish(exchange: queue.Exchange,
                                     routingKey: queue.RoutingKey,
                                     mandatory: default(bool),
                                     basicProperties: null,
                                     body: body);
            }
        }

        public Task SubscribeAsync<T>(string topic, IActor<T> actor)
        {
            return Subscriptions.SubscribeAsync(topic, actor);
        }

        private async Task<byte[]> Serialize<T>(T message)
        {
            using (var buffer = await MemoryStreamPool.GetMemoryStream())
            {
                Serializer.Serialize<T>(buffer, message);
                return buffer.ToArray();
            }
        }

        private int _isStopped = 0;

        public Task StopAsync()
        {
            if (Interlocked.CompareExchange(ref _isStopped, 1, 0) == 0)
            {
                Subscriptions.Dispose();
                Connections.Dispose();
            }
            return Task.CompletedTask;
        }
    }
}
