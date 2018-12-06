using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.ApplicationEventBus;

namespace Tavisca.Common.Plugins.Configuration
{
    /// <summary>
    /// event channel to handle events raised by locally
    /// </summary>
    public class LocalEventChannel : IEventChannel
    {
        IEventChannel _eventBus { get; set; }

        public LocalEventChannel() : this(new RXEventChannel())
        {

        }

        public LocalEventChannel(IEventChannel eventBus)
        {
            _eventBus = eventBus;
        }
        public IObservable<ApplicationEvent> GetChannel(string eventName)
        {
            return null;
            // DO Nothing
        }

        public void Notify(ApplicationEvent eventData)
        {
            _eventBus.Notify(eventData);
        }
      
    }
}
