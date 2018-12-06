using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.SessionStore
{
    public interface ISessionStore
    {
        Task AddAsync<T>(DataItem<T> item, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true);
        Task<T> GetAsync<T>(ItemKey itemKey, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true);
        Task AddMultipleAsync<T>(DataItem<T>[] items, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true);
        Task<List<DataItem<T>>> GetMultipleAsync<T>(ItemKey[] itemKeys, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true);
    }
}
