using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
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
using Tavisca.Platform.Common.Context;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Serialization;
using Tavisca.Platform.Common.ServiceBus;



namespace Tavisca.Common.Plugins.RabbitMq
{
    internal class ConnectionPool : IDisposable
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        private readonly ConcurrentDictionary<Queue, IConnection> _pool = new ConcurrentDictionary<Queue, IConnection>(Queue.QueueComparer);
        private readonly InstanceEventBus _eventBus = new InstanceEventBus();

        public IModel CreateChannel(Queue queue)
        {
            IConnection connection;
            if ((_pool.TryGetValue(queue, out connection) == false) || connection?.IsOpen == false)
            {
                _lock.EnterWriteLock();
                try
                {
                    if ((_pool.TryGetValue(queue, out connection) == false) || connection?.IsOpen == false)
                    {
                        connection = queue.CreateConnection();
                        RegisterConnectionRecoveryEvents(connection, queue);
                        _pool[queue] = connection;
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            
            return connection.CreateModel();
        }



        private void RegisterConnectionRecoveryEvents(IConnection connection, Queue queue)
        {
            connection.RecoverySucceeded += (model, eventArgs) =>
            {
                TraceConnectionRecovery(queue);
            };

            connection.ConnectionRecoveryError += (model, eventArgs) =>
            {
                TriggerManualReconnection(connection, queue);
            };
        }

        private void TraceConnectionRecovery(Queue queue)
        {
            var context = CallContext.Current;
            var log = new TraceLog
            {
                ApplicationName = context?.ApplicationName,
                Category = "rabbitmq_node_recovery",
                Message = $"rabbitmq connection for the queue {queue.GetQueueIdentifier()} has been recovered"
            };

            Logger.WriteLogAsync(log);
        }

        // Create a policy which will keep trying to connect to rmq in incremental time durations 
        // once connected we will need to refresh the cache entry

        private void TriggerManualReconnection(IConnection connection, Queue queue)
        {
            IConnection newConnection = null;
            // TODO need to define retry criteria
            var policy = Policy.Handle<BrokerUnreachableException>()
                               .Or<OperationInterruptedException>()
                               .WaitAndRetryForever(retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            policy.Execute(() => { newConnection = queue.CreateConnection(); });

            if (newConnection?.IsOpen == true)
            {
                // Whenever there is a need for manual recovery the connection once recovered,
                // will replace the older connection which will happen for every queue whose 
                // corresponding connection is broken
                RegisterConnectionRecoveryEvents(newConnection, queue);
                _pool[queue] = newConnection;               
                _eventBus.Notify(GetRmqReconnectionEvent(queue));
            }

        }

        private ApplicationEvent GetRmqReconnectionEvent(Queue queue)
        {
            return new ApplicationEvent
            {
                Id = Guid.NewGuid().ToString(),
                Name = KeyStore.ConnectionRefereshEvent,
                TimeStamp = DateTime.Now,
                Context = "refresh rmq subscription " + queue.GetQueueIdentifier()
            };
        }

        private int _isDisposed = 0;
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 0)
            {
                foreach (var item in _pool.ToArray())
                    item.Value.Close();
                _pool.Clear();
            }
        }
    }
}
