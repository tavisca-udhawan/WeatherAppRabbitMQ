using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.ApplicationEventBus
{
    public interface IApplicationEventBus
    {
        void Register(string eventName, ApplicationEventObserver applicationEventObserver);

        void Notify(ApplicationEvent eventData);
    }
}
