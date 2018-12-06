using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.Configuration;
using Tavisca.Platform.Common;
using Tavisca.Platform.Common.ApplicationEventBus;
using Tavisca.Platform.Common.Context;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Serialization;
using Tavisca.Platform.Common.ServiceBus;



namespace Tavisca.Common.Plugins.RabbitMq
{
    internal class RmqSubscription<T> : ISubscription
    {
        private IActor<T> _actor;
        private IModel _channel;
        private readonly string _topic;
        private readonly RmqBus _bus;
        private ReadQueue _queue;
        private bool _isDisposing = false;
        private bool _stopListenting = false;
        private List<Task> _tasks = new List<Task>();
        private static readonly object _lock = new object();
        public RmqSubscription(string topic, RmqBus bus, IActor<T> actor, ReadQueue queue)
        {
            _topic = topic;
            _bus = bus;
            _queue = queue;
            _actor = actor;
            AttachTo(bus);

        }

        public ISubscription CreateSubscription(ReadQueue queue)
        {
            return new RmqSubscription<T>(_topic, _bus, _actor, queue);
        }

        public void Stop()
        {
            /*
             * set the flag, so that no more messages are processed by the channel
             * dispose the channel once all tasks started by the channel are over.
             */
            _stopListenting = true;
            Task.WhenAll(_tasks)
                .ContinueWith(t =>
                {
                    Dispose();
                });
        }



        private void AttachTo(IBus bus)
        {
            //register bus and register the subscription for consul refresh event
            _channel = ProcessChannel(bus, _queue);
        }

        private IModel ProcessChannel(IBus bus, ReadQueue queue)
        {
            var channel = _bus.Connections.CreateChannel(queue);
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                Notify(bus, ea);
            };
            consumer.Shutdown += (s, e) =>
            {
                TraceShutDownEvent(e);
            };
            channel.BasicQos(0, queue.PrefetchSize, false);

            //the argument noAck has been renamed to autoAck from RabbitMq.Client(5.0.1)
            //for reference https://github.com/rabbitmq/rabbitmq-dotnet-client/issues/255

            channel.BasicConsume(queue: queue.Name, autoAck: false, consumer: consumer);
            return channel;
        }

        private void TraceShutDownEvent(ShutdownEventArgs e)
        {
            var context = CallContext.Current;
            var log = new TraceLog
            {
                ApplicationName = context?.ApplicationName,
                Category = "rabbitmq_node_shutdown",
                Message = "rabbitmq node shutdown event occured",               
            };
            log.SetValue("initiated_by", e.Initiator.ToString());
            Logger.WriteLogAsync(log);
        }

        public void Notify(IBus bus, BasicDeliverEventArgs msg)
        {
            if (_stopListenting)
            {
                _channel?.BasicReject(msg.DeliveryTag, true);
                return;
            }
            T payload;
            // Reject any messages that cannot be deserialized since we know that we will not be able to process them.
            if (TryDeserialize(msg, out payload) == false)
            {
                _channel.BasicReject(msg.DeliveryTag, false);
                return;
            }
            var message = new RmqMessage<T>(bus, payload, _channel, msg.DeliveryTag);

            var task = Task.Factory.StartNew(async () => await _actor.NotifyAsync(message));
            task.ContinueWith(t => RemoveTask(task));
            AddTask(task);
        }

        private bool TryDeserialize<T>(BasicDeliverEventArgs msg, out T result)
        {
            try
            {
                using (var buffer = new MemoryStream(msg.Body))
                {
                    result = _bus.Serializer.Deserialize<T>(buffer);
                }
                return true;
            }
            catch
            {
                result = default(T);
                return false;
            }
        }

        private void AddTask(Task task)
        {
            if (task == null)
                return;

            lock (_lock)
            {
                _tasks.Add(task);

            }
        }
        private void RemoveTask(Task task)
        {
            lock (_lock)
            {
                _tasks.Remove(task);
            }

        }

        public void Dispose()
        {
            _isDisposing = true;
            var channel = _channel;
            if (_channel != null)
            {
                _channel = null;
                channel.Dispose();
            }

            if (_stopListenting == false)
            {
                var actor = _actor;
                if (actor != null)
                {
                    _actor = null;
                    actor.Dispose();
                }
            }
        }
    }

}
