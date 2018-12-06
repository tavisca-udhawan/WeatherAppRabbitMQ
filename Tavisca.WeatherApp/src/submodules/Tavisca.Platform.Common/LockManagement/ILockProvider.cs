using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.LockManagement
{
    public interface ILockProvider
    {
        Task<bool> TryGetLockAsync(string lockId, LockType lockType, CancellationToken cancellationToken);
        Task ReleaseLockAsync(string lockId, LockType lockType, CancellationToken cancellationToken);
    }
}
