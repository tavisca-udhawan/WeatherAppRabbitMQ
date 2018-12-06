using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.ApplicationEventBus;

namespace Tavisca.Common.Plugins.Configuration
{
    public class ApplicationEventBus : IApplicationEventBus
    {
        private IEventChannel _executionChannel;

        private IEventChannel _distributionChannel;

        public ApplicationEventBus() : this(new RXEventChannel(), new SignalREventChannel())
        {

        }
        public ApplicationEventBus(IEventChannel executionChannel, IEventChannel distributionChannel)
        {
            _executionChannel = executionChannel;
            _distributionChannel = distributionChannel;
        }
        public void Register(string eventName, ApplicationEventObserver applicationEventObserver)
        {
            var channel = _executionChannel.GetChannel(eventName);
            channel.Subscribe(applicationEventObserver);
        }

        public void Notify(ApplicationEvent eventData)
        {
            _distributionChannel.Notify(eventData);
        }
    }
}
