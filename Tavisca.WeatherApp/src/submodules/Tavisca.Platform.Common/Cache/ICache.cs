using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Cache
{
    public interface ICache<T>
    {

        Task<T> GetAsync(string key, CancellationToken cancellationToken = default(CancellationToken), Func<byte[], T> deserialize = null);

        Task SetAsync(string key, T value, CancellationToken cancellationToken = default(CancellationToken), Func<T, byte[]> serialize = null, TimeSpan? expiresIn = null);

        Task DeleteAsync(string key, CancellationToken cancellationToken = default(CancellationToken));

        Task<List<CacheHit<T>>> MultiGetAsync(List<string> keys, CancellationToken cancellationToken = default(CancellationToken), Func<byte[], T> deserialize = null);

    }
}
