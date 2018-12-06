using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common
{
    public interface IHttpConnector
    {
        Task<HttpResponse> SendAsync(HttpRequest request, CancellationToken cancellationToken = default(CancellationToken));
    }
}


