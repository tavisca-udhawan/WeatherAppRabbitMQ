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
using Tavisca.Platform.Common.Serialization;
using Tavisca.Platform.Common.ServiceBus;



namespace Tavisca.Common.Plugins.RabbitMq
{
    public class RmqMessage<T> : IMessageWithRejectionSupport<T>
    {
        private readonly IModel _channel;
        private readonly ulong _deliveryTag;

        public RmqMessage( IBus bus, T payload, IModel channel, ulong deliveryTag)
        {
            Bus = bus;
            Payload = payload;
            _channel = channel;
            _deliveryTag = deliveryTag;
        }

        public IBus Bus { get; }

        public T Payload
        {
            get;
        }

        private int _isCompleted = 0;
        public Task CompleteAsync()
        {
            Complete();
            return Task.CompletedTask;
        }

        public Task RejectAsync()
        {
            Reject();   
            return Task.CompletedTask;
        }

        public void Reject()
        {
            if (Interlocked.CompareExchange(ref _isCompleted, 1, 0) == 0 && _channel.IsClosed == false)
            {
                lock (_channel)
                {
                    _channel.BasicReject(_deliveryTag, false);
                }
            }
        }

        public void Complete()
        {
            if (Interlocked.CompareExchange(ref _isCompleted, 1, 0) == 0 && _channel.IsClosed == false)
            {
                lock (_channel)
                {
                    _channel.BasicAck(_deliveryTag, false);
                }
            }
        }
    }
    
}
