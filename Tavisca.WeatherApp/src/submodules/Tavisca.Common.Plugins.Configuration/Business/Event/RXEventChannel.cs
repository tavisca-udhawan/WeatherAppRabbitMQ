using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.ApplicationEventBus;

namespace Tavisca.Common.Plugins.Configuration
{
    public sealed class RXEventChannel : IEventChannel, IDisposable
    {
        private readonly static ConcurrentDictionary<string, Subject<ApplicationEvent>> _channels =
            new ConcurrentDictionary<string, Subject<ApplicationEvent>>(StringComparer.OrdinalIgnoreCase);

        public IObservable<ApplicationEvent> GetChannel(string channelName)
        {
            var channel = _channels.GetOrAdd(channelName, t => new Subject<ApplicationEvent>());
            return channel as IObservable<ApplicationEvent>;
        }

        public void Notify(ApplicationEvent eventData)
        {
            if (eventData == null || string.IsNullOrWhiteSpace(eventData.Name))
                return;

            Subject<ApplicationEvent> value = null;
            if (_channels.TryGetValue(eventData.Name, out value) == false)
                return;
            else
            {
                var channel = value as Subject<ApplicationEvent>;
                if (channel != null)
                    channel.OnNext(eventData);
            }

        }

        public void Dispose()
        {
            var channels = _channels.Values.Cast<IDisposable>().ToList();
            _channels.Clear();
            channels.ForEach(c => c.Dispose());
        }

    }
}
