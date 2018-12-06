using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.ApplicationEventBus;

namespace Tavisca.Common.Plugins.Configuration
{
    public interface IEventChannel
    {
        IObservable<ApplicationEvent> GetChannel(string eventName);

        void Notify(ApplicationEvent eventData);
    }
}
