using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.ServiceBus
{
    public abstract class Actor<T> : IActor<T>
    {
        public abstract void Dispose();

        protected abstract Task OnMessageAsync(IBus bus, T message);

        async Task IActor<T>.NotifyAsync(IMessageWithRejectionSupport<T> msg)
        {
            try
            {
                await OnMessageAsync(msg.Bus, msg.Payload);
            }
            catch (Exception ex)
            {
                await msg.RejectAsync();
                return;
            }
            await msg.CompleteAsync();
        }
    }

}
