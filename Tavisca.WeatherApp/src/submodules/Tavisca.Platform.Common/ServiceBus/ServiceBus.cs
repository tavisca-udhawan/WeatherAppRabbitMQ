using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.ServiceBus
{
    public class ServiceBus
    {   
        public static ServiceBus Create(IBus bus)
        {
            return new ServiceBus(bus);
        }

        private readonly IBus _bus;
        private ServiceBus(IBus bus)
        {
            _bus = bus;
        }

        public Task PublishAsync<T>(string topic, T message)
        {
            return _bus.PublishAsync(topic, message);
        }

        public Task SubscribeAsync<T>(string topic, IActor<T> observer)
        {
            return _bus.SubscribeAsync(topic, observer);
        }

        public Task StopAsync()
        {
            return _bus.StopAsync();
        }
    }
}
