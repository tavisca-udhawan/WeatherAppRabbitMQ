using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common
{
    internal class HttpConnectorFilter : HttpFilter
    {
        private IHttpConnector _connector;
        protected internal HttpConnectorFilter(IHttpConnector connector)
        {
            _connector = connector;
        }
        public async override  Task<HttpResponse> ApplyAsync(HttpRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return await _connector.SendAsync(request, cancellationToken);
        }
    }
}
