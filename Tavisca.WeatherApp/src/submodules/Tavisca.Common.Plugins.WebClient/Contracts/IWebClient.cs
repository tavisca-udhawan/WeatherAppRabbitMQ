using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.WebClient
{
    public interface IWebClient : IDisposable
    {
        Task<WebClientResponseMessage> GetAsync(WebClientRequestMessage request, CancellationToken cancellationToken);

        Task<WebClientResponseMessage> PostAsync(WebClientRequestMessage request, CancellationToken cancellationToken);

        Task<WebClientResponseMessage> PutAsync(WebClientRequestMessage request, CancellationToken cancellationToken);

        Task<WebClientResponseMessage> DeleteAsync(WebClientRequestMessage request, CancellationToken cancellationToken);

        Task<WebClientResponseMessage> PatchAsync(WebClientRequestMessage request, CancellationToken cancellationToken);
    }
}
