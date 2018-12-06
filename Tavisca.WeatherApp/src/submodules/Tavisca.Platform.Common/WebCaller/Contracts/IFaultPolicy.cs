using System.Threading.Tasks;

namespace Tavisca.Platform.Common
{
    public interface IFaultPolicy
    {
        Task<bool> IsFaultedAsync(HttpRequest req, HttpResponse res);
    }
    

}
