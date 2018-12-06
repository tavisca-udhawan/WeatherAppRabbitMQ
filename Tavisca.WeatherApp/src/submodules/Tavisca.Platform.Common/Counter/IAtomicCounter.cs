using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Counter
{
    public interface IAtomicCounter
    {
        Task Create(string id, TimeSpan expiry, long defaultValue = 0, CancellationToken cancellationToken = default(CancellationToken));
        Task<long> GetCurrentValue(string id, CancellationToken cancellationToken = default(CancellationToken));
        Task<long> Increment(string id, int incrementFactor = 1, CancellationToken cancellationToken = default(CancellationToken));
        Task<long> Decrement(string id, int decrementFactor = 1, CancellationToken cancellationToken = default(CancellationToken));
    }
}
