using System;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Serialization;

namespace Tavisca.Platform.Common.ServiceBus
{
    public interface IBus
    {
        Task PublishAsync<T>(string topic, T message);
        Task SubscribeAsync<T>(string topic, IActor<T> observer);
        Task StopAsync();
    }
}