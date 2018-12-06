using System;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.WebClient;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    internal class HttpClient : Client
    {
        public override Task<WebClientResponseMessage> GetAsync(WebClientRequestMessage request, ClientSetting clientSetting, CancellationToken cancellationToken)
        {
            var client =WebClientFactory.Create(new Settings() { TimeOut = (int)clientSetting.TimeOut.TotalMilliseconds });
            return client.GetAsync(request, cancellationToken);

        }

        public override Task<WebClientResponseMessage> PostAsync(WebClientRequestMessage request, ClientSetting clientSetting, CancellationToken cancellationToken)
        {
            var client = WebClientFactory.Create(new Settings() { TimeOut = (int)clientSetting.TimeOut.TotalMilliseconds });
            return client.PostAsync(request, cancellationToken);
        }

        public override Task<WebClientResponseMessage> PutAsync(WebClientRequestMessage request, ClientSetting clientSetting, CancellationToken cancellationToken)
        {
            var client = WebClientFactory.Create(new Settings() { TimeOut = (int)clientSetting.TimeOut.TotalMilliseconds });
            return client.PutAsync(request, cancellationToken);
        }

        public override Task<WebClientResponseMessage> DeleteAsync(WebClientRequestMessage request, ClientSetting clientSetting, CancellationToken cancellationToken)
        {
            var client = WebClientFactory.Create(new Settings() { TimeOut = (int)clientSetting.TimeOut.TotalMilliseconds });
            return client.DeleteAsync(request, cancellationToken);
        }

        public override Task<WebClientResponseMessage> PatchAsync(WebClientRequestMessage request, ClientSetting clientSetting, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }


    }
}
