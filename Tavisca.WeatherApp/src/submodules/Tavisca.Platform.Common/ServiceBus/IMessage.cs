using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.ServiceBus
{
    public interface IMessage<out T>
    {
        IBus Bus { get; }

        T Payload { get; }

        Task CompleteAsync();

        void Complete();
    }

}
