using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;

namespace Tavisca.Platform.Common.ServiceBus
{
    public class InProcBus : IBus
    {
        private readonly ConcurrentDictionary<string, IDisposable> _pool = new ConcurrentDictionary<string, IDisposable>(StringComparer.OrdinalIgnoreCase);

        public Task PublishAsync<T>(string topic, T message)
        {
            IDisposable disposable;

            if (_pool.TryGetValue(topic, out disposable) == true)
                ((Queue<T>) disposable).Enqueue(message);

            return Task.CompletedTask;
        }

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
        public Task SubscribeAsync<T>(string topic, IActor<T> observer)
        {
            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_pool.ContainsKey(topic) == true)
                    throw new Exception("Topic already registered.");
                _lock.EnterWriteLock();
                try
                {
                    if (_pool.ContainsKey(topic) == true)
                        throw new Exception("Topic already registered.");
                    var queue = new Queue<T>();
                    _pool[topic] = queue;
                    #pragma warning disable 4014
                    Task.Factory.StartNew(() => queue.StartAsync(this, observer));
                    #pragma warning restore 4014
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
            return Task.CompletedTask;
        }
        #pragma warning restore 1998

        public Task StopAsync()
        {
            foreach(var pool in _pool)
            {
                pool.Value.Dispose();
            }
            return Task.CompletedTask;
        }

        private class Queue<T> : IDisposable
        {
            private readonly BlockingCollection<T> _queue;

            public Queue()
            {
                _queue = new BlockingCollection<T>(new ConcurrentQueue<T>());
            }

            public async Task StartAsync(IBus bus, IActor<T> actor)
            {
                foreach (var item in _queue.GetConsumingEnumerable())
                    await actor.NotifyAsync(new Message<T>(bus, item));
            }

            public void Enqueue(T item)
            {
                _queue.Add(item);
            }
             
            public void Dispose()
            {
                _queue.CompleteAdding();
                _queue.Dispose();
            }
        }

        private class Message<T> : IMessageWithRejectionSupport<T>
        {
            public Message(IBus bus, T item)
            {
                Bus = bus;
                Payload = item;
            }

            public IBus Bus { get; }

            public T Payload { get; }

            public void Complete()
            {
            }

            public Task CompleteAsync()
            {
                return Task.CompletedTask;
            }

            public void Reject()
            {
            }

            public Task RejectAsync()
            {
                return Task.CompletedTask;
            }
        }
    }
}
