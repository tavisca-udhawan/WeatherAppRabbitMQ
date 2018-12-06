using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.Configuration;
using Tavisca.Platform.Common.ApplicationEventBus;
using Tavisca.Platform.Common.Serialization;
using Tavisca.Platform.Common.ServiceBus;



namespace Tavisca.Common.Plugins.RabbitMq
{
    internal class RmqSubscriptions : ApplicationEventObserver, IDisposable
    {

        public RmqSubscriptions(RmqBus bus)
        {
            _bus = bus;
            // in-memory-consul-cache-refresh is raised on consul refersh 
            //it's registered on local channel .
            var applicationBus = new InstanceEventBus();
            applicationBus.Register("in-memory-consul-cache-refresh", this);
            applicationBus.Register(KeyStore.ConnectionRefereshEvent, this);

        }

        private readonly AwaitableLock _lock = new AwaitableLock();
        private readonly Dictionary<string, Tuple<ReadQueue, ISubscription>> _pool = new Dictionary<string, Tuple<ReadQueue, ISubscription>>(StringComparer.OrdinalIgnoreCase);
        private readonly RmqBus _bus;
        private static readonly object _syncLock = new object();

        public async Task SubscribeAsync<T>(string topic, IActor<T> actor)
        {
            using (await _lock.LockAsync())
            {
                if (_pool.ContainsKey(topic) == true)
                    throw new Exception("Topic already registered with existing actor.");
                var queue = await _bus.Settings.GetReadQueueAsync(topic);
                var rmqSubscription = new RmqSubscription<T>(topic, _bus, actor, queue);
                _pool[topic] = new Tuple<ReadQueue, ISubscription>(queue, rmqSubscription);
            }

        }

        public override void Process(ApplicationEvent msg)
        {
            lock (_syncLock)
            {
                foreach (var subscription in _pool.ToList())
                {
                    var newSettings = _bus.Settings.GetReadQueueAsync(subscription.Key).GetAwaiter().GetResult();
                    var oldSettings = subscription.Value.Item1;
                    var hasSettingsChanged = oldSettings.Equals(newSettings) == false;
                    if (msg.Name == KeyStore.ConnectionRefereshEvent || hasSettingsChanged)
                        ReplaceOldSubscription(subscription.Key, subscription.Value.Item2, newSettings);

                }
            }
        }
        private void ReplaceOldSubscription(string topic, ISubscription subscription, ReadQueue newSettings)
        {
            /*create a new connection for same topic with new settings
             * stop accepting new messages on old channel
             */
            _pool[topic] = new Tuple<ReadQueue, ISubscription>(newSettings, subscription.CreateSubscription(newSettings));      
            subscription.Stop();
           
        }


        private int _isDisposed = 0;

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
            {
                _pool
                    .AsEnumerable()
                    .ToList()
                    .ForEach(x => x.Value.Item2.Dispose());
            }
        }
    }
}
