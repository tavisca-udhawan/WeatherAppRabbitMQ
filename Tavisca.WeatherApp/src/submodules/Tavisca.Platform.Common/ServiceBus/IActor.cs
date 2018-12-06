using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.ServiceBus
{
    public interface IActor<in T> : IDisposable
    {
        Task NotifyAsync(IMessageWithRejectionSupport<T> msg);
    }
    

}
