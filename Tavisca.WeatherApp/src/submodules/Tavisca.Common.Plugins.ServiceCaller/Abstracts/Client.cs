using System.Threading;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.WebClient;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    public abstract class Client
    {

        public abstract Task<WebClientResponseMessage> GetAsync(WebClientRequestMessage request, ClientSetting clientSetting,
            CancellationToken cancellationToken);

        public abstract Task<WebClientResponseMessage> PostAsync(WebClientRequestMessage request, ClientSetting clientSetting,
            CancellationToken cancellationToken);


        public abstract Task<WebClientResponseMessage> PutAsync(WebClientRequestMessage request, ClientSetting clientSetting,
            CancellationToken cancellationToken);


        public abstract Task<WebClientResponseMessage> DeleteAsync(WebClientRequestMessage request, ClientSetting clientSetting,
            CancellationToken cancellationToken);


        public abstract Task<WebClientResponseMessage> PatchAsync(WebClientRequestMessage request, ClientSetting clientSetting,
            CancellationToken cancellationToken);

    }
}
