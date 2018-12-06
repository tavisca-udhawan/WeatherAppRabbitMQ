using System;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common
{
    internal class DelegatedFaultPolicy : IFaultPolicy
    {
        Func<HttpRequest, HttpResponse, Task<bool>> _isFaulted;
        public DelegatedFaultPolicy(Func<HttpRequest, HttpResponse,Task<bool>> func)
        {
            _isFaulted = func;
        }

        public async Task<bool> IsFaultedAsync(HttpRequest req, HttpResponse res)
        {
            return await _isFaulted(req, res);
        }
    }
    

}
