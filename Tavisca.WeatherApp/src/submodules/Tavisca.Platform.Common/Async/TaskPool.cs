using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Internal
{
    public class TaskPool : ITaskPool
    {
        public readonly BlockingCollection<Action> _queue = null;
        private Task[] _tasks;

        public TaskPool(int size)
        {
            _queue = new BlockingCollection<Action>(new ConcurrentQueue<Action>());
            _tasks = new Task[size];
            for (int i = 0; i < size; i++)
            {
                _tasks[i] = Task.Factory.StartNew( () =>
               {
                   foreach( var work in _queue.GetConsumingEnumerable() )
                   {
                       try
                       {
                           work();
                       }
                       catch { }
                   }
               }, TaskCreationOptions.LongRunning);
            }
        }


        public void Enqueue(Action action)
        {
            _queue.Add(action);
        }

        public void StopAdding()
        {
            _queue.CompleteAdding();
        }
    }
}
