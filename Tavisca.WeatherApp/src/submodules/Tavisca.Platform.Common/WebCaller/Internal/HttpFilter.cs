using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Logging;

namespace Tavisca.Platform.Common
{
    public abstract class HttpFilter
    {
        private HttpFilter _innerFilter;

        internal void SetInnerFilter(HttpFilter innerFilter)
        {
            _innerFilter = innerFilter;
        }
        public virtual async Task<HttpResponse> ApplyAsync(HttpRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _innerFilter != null ? await _innerFilter.ApplyAsync(request, cancellationToken) : null;
        }
    }
}
