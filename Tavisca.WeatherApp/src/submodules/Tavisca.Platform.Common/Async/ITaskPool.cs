using System;

namespace Tavisca.Platform.Common
{
    public interface ITaskPool
    {
        void Enqueue(Action action);
        void StopAdding();
    }
}