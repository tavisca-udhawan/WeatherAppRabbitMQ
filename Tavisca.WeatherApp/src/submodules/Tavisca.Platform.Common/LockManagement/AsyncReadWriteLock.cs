using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Tavisca.Platform.Common
{
    public sealed class AsyncReadWriteLock
    {
        private int status;
        private int waitingReadsCount;
        private readonly Task<IDisposable> readReleaser;
        private readonly Task<IDisposable> writeReleaser;        
        private TaskCompletionSource<IDisposable> waitingReads = new TaskCompletionSource<IDisposable>();
        private readonly Queue<TaskCompletionSource<IDisposable>> waitingWrites = new Queue<TaskCompletionSource<IDisposable>>();

        public AsyncReadWriteLock()
        {
            readReleaser = Task.FromResult((IDisposable)new Releaser(this, false));
            writeReleaser = Task.FromResult((IDisposable)new Releaser(this, true));
        }

        public Task<IDisposable> ReadLockAsync()
        {
            lock (waitingWrites)
            {
                if (status >= 0 && waitingWrites.Count == 0)
                {
                    ++status;
                    return readReleaser;
                }
                else
                {
                    ++waitingReadsCount;
                    return waitingReads.Task.ContinueWith(t => t.Result);
                }
            }
        }

        public Task<IDisposable> WriteLockAsync()
        {
            lock (waitingWrites)
            {
                if (status == 0)
                {
                    status = -1;
                    return writeReleaser;
                }
                else
                {
                    var waitingWrite = new TaskCompletionSource<IDisposable>();
                    waitingWrites.Enqueue(waitingWrite);
                    return waitingWrite.Task;
                }
            }
        }

        private void ReleaseRead()
        {
            TaskCompletionSource<IDisposable> toWake = null;

            lock (waitingWrites)
            { 
                --status;
                if (status == 0 && waitingWrites.Count > 0)
                {
                    status = -1;
                    toWake = waitingWrites.Dequeue();

                    if (toWake != null)
                        toWake.SetResult(new Releaser(this, true));
                }
            }            
        }

        private void ReleaseWrite()
        {
            TaskCompletionSource<IDisposable> toWake = null;
            bool isWrite = false;

            lock (waitingWrites)
            {
                if (waitingWrites.Count > 0)
                {
                    toWake = waitingWrites.Dequeue();
                    isWrite = true;
                }
                else if (waitingReadsCount > 0)
                {
                    toWake = waitingReads;
                    status = waitingReadsCount;
                    waitingReadsCount = 0;
                    waitingReads = new TaskCompletionSource<IDisposable>();
                }
                else
                    status = 0;

                if (toWake != null)
                    toWake.SetResult(new Releaser(this, isWrite));
            }            
        }

        private sealed class Releaser : IDisposable
        {
            private readonly AsyncReadWriteLock _toRelease;
            private readonly bool _isWriteLock;

            internal Releaser(AsyncReadWriteLock toRelease, bool isWriteLock)
            {
                _toRelease = toRelease;
                _isWriteLock = isWriteLock;
            }

            public void Dispose()
            {
                if (_isWriteLock)
                    _toRelease.ReleaseWrite();
                else
                    _toRelease.ReleaseRead();
            }
        }
    }
}