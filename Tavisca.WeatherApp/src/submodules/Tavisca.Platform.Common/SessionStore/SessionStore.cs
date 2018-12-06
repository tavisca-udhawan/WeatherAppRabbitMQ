using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.SessionStore;
using System.Collections.Generic;

namespace Tavisca.Common.Plugins.SessionStore
{
    public class SessionStore : ISessionStore
    {
        /*
         * Provider will be created per call 
         * and all session configurations will be fetched by provider as required.
         */
         
        private readonly ISessionProviderFactory _sessionProviderFactory;

        public SessionStore(ISessionProviderFactory sessionProviderFactory)
        {
            _sessionProviderFactory = sessionProviderFactory;
        }
        public async Task AddAsync<T>(DataItem<T> item, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true)
        {
            var provider = _sessionProviderFactory.GetSessionProvider();
            await provider.AddAsync(item.Key, item.Value, cancellationToken, enableTraces);
        }

        public async Task AddMultipleAsync<T>(DataItem<T>[] items, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true)
        {

            var provider = _sessionProviderFactory.GetSessionProvider();
            await provider.AddMultipleAsync(items, cancellationToken, enableTraces);
        }

        public async Task<List<DataItem<T>>> GetMultipleAsync<T>(ItemKey[] itemKeys, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true)
        {
            var provider = _sessionProviderFactory.GetSessionProvider();
            return await provider.GetAllAsync<T>(itemKeys, cancellationToken, enableTraces);
        }

        public async Task<T> GetAsync<T>(ItemKey itemKey, CancellationToken cancellationToken = default(CancellationToken), bool enableTraces = true)
        {
            var provider = _sessionProviderFactory.GetSessionProvider();
            return await provider.GetAsync<T>(itemKey, cancellationToken, enableTraces);
        }
    }
}
