using System;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common
{
    internal class DefaultFaultPolicy : IFaultPolicy
    {
        public static IFaultPolicy Instance = new DefaultFaultPolicy();

        private DefaultFaultPolicy() { }

        public Task<bool> IsFaultedAsync(HttpRequest req, HttpResponse res)
        {
            var statusCode = (int)(res?.Status);
            return Task.FromResult(statusCode / 100 == 2);
        }
    }
    

}
