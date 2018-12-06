using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Internal
{
    public class RoundRobinPool : ITaskPool
    {
        public RoundRobinPool(int maxThreads)
        {
            if (maxThreads <= 0)
                throw new ArgumentException($"{nameof(maxThreads)} cannot be less than or equal to zero.");
            _queues = new ConcurrentQueue<Action>[maxThreads];
            _tasks = new Task[maxThreads];
            var waitHandle = new CountdownEvent(maxThreads);
            for (int i = 0; i < maxThreads; i++)
            {
                _tasks[i] = Task.Factory.StartNew(() =>
                {
                    waitHandle.Signal();
                    foreach (var work in this.GetIterator())
                    {
                        try
                        {
                            work();
                        }
                        catch { }
                    }
                }, TaskCreationOptions.LongRunning);
            }
            // Wait for all workers to start.
            waitHandle.Wait();
        }

        private readonly Task[] _tasks;
        private readonly ConcurrentQueue<Action>[] _queues;
        private int _inc = -1;
        private int _subscriberCount = 0;
        private bool _isComplete = false;
        
        
        public void Enqueue(Action item)
        {
            if (_isComplete == true)
                throw new InvalidOperationException("Cannot enqueue items as pool has been marked as completed.");
            var queue = GetNextQueue();
            queue?.Enqueue(item);
        }

        public void StopAdding()
        {
            _isComplete = true;
        }

        private ConcurrentQueue<Action> GetNextQueue()
        {
            if (_subscriberCount == 0)
                return null;
            uint value = unchecked((uint)(Interlocked.Increment(ref _inc)));
            int index = (int)(value % _subscriberCount);
            return _queues[index];
        }

        private IEnumerable<Action> GetIterator()
        {
            var queue = new ConcurrentQueue<Action>();
            _queues[_subscriberCount] = queue;
            _subscriberCount++;
            return GenerateIterator(queue);
        }

        private IEnumerable<Action> GenerateIterator(ConcurrentQueue<Action> queue)
        {
            Action item = null;
            SpinWait spinner = new SpinWait();
            while(true)
            {
                item = null;
                if (queue.TryDequeue(out item) == true)
                    yield return item;
                else
                {
                    if (_isComplete == true)
                        yield break;
                    spinner.SpinOnce();
                }
            }
        }
    }
}
