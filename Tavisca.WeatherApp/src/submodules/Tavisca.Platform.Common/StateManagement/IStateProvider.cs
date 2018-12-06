using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.StateManagement
{
    public interface IStateProvider
    {
        Task<object> GetStateAsync(string stateId, CancellationToken cancellationToken);
        Task SaveStateAsync(string stateId, object state, CancellationToken cancellationToken, int ExpiryTime = 3600);
        Task<bool> Exists(string stateId, CancellationToken cancellationToken);
    }
}
