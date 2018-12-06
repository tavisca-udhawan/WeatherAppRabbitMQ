using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.LockManagement
{
    public sealed class GlobalLock
    {
        private readonly ILockProvider _lockProvider;
        private readonly IRetryController _retryController;
        
        public GlobalLock(ILockProvider lockProvider, IRetryController retryController = null)
        {
            _lockProvider = lockProvider;
            _retryController = retryController ?? new LinearRetryController();
        }

        public async Task<IDisposable> EnterReadLock(string id, CancellationToken? cancellationToken = null)
        {
            int? retryTimeout;
            bool lockAcquired;

            do
            {
                lockAcquired = await _lockProvider.TryGetLockAsync(id, LockType.Read, cancellationToken ?? CancellationToken.None);
                if (lockAcquired) break;
                retryTimeout = _retryController.GetNextRetryInterval();
                if (retryTimeout.HasValue)
                    await Task.Delay(retryTimeout.Value, cancellationToken ?? CancellationToken.None);
            } while (retryTimeout != null);

            if (!lockAcquired)
                throw new TimeoutException("Get read lock timeout reached.");

            return new DisposableAction(() =>
            _lockProvider.ReleaseLockAsync(id, LockType.Read, cancellationToken ?? CancellationToken.None));
        }

        public async Task<IDisposable> EnterWriteLock(string id, CancellationToken? cancellationToken = null)
        {
            int? retryTimeout;
            bool lockAcquired;

            do
            {
                lockAcquired = await _lockProvider.TryGetLockAsync(id, LockType.Write, cancellationToken ?? CancellationToken.None);
                if (lockAcquired) break;
                retryTimeout = _retryController.GetNextRetryInterval();
                if (retryTimeout.HasValue)
                    await Task.Delay(retryTimeout.Value, cancellationToken ?? CancellationToken.None);
            } while (retryTimeout != null);

            if (!lockAcquired)
                throw new TimeoutException("Get write lock timeout reached.");
            
            return new DisposableAction(() =>
            _lockProvider.ReleaseLockAsync(id, LockType.Write, cancellationToken ?? CancellationToken.None));
        }
    }

    internal class DisposableAction : IDisposable
    {
        private readonly Action _disposeAction;

        public DisposableAction(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            _disposeAction();
        }
    }
}
