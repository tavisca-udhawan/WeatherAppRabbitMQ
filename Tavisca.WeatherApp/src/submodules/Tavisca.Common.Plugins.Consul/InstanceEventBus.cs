using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.ApplicationEventBus;

namespace Tavisca.Common.Plugins.Configuration
{
    /// <summary>
    /// event bus implementation which will distribute events on local channel (i.e., these events won't be published on network)
    /// LocalEventChannel is distributionChannel, which takes care of publishing events locally
    /// </summary>
    public class InstanceEventBus : IApplicationEventBus
    {
        private IEventChannel _executionChannel;

        private IEventChannel _distributionChannel;

        public InstanceEventBus() : this(new RXEventChannel(), new LocalEventChannel())
        {

        }
        public InstanceEventBus(IEventChannel executionChannel, IEventChannel distributionChannel)
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
