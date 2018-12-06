using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.ServiceBus
{
    public interface IMessageWithRejectionSupport<out T> : IMessage<T>
    {
        Task RejectAsync();

        void Reject();
    }
}
