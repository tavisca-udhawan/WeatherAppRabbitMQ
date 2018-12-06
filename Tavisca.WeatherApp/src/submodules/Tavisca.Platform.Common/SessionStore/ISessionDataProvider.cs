using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.SessionStore;

namespace Tavisca.Common.Plugins.SessionStore
{
    public interface ISessionDataProvider
    {
        Task AddAsync<T>(ItemKey itemKey, T value, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true);
        Task AddMultipleAsync<T>(DataItem<T>[] items, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true);
        Task<List<DataItem<T>>> GetAllAsync<T>(ItemKey[] sessionKeys, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true);
        Task<T> GetAsync<T>(ItemKey sessionKey, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true);
        Task<bool> RemoveAsync(ItemKey itemKey, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true);


    }
}
