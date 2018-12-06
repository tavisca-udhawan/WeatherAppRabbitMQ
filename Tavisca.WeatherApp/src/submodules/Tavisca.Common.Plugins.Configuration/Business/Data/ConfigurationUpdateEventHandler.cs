using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Tavisca.Platform.Common;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.Containers;
using Tavisca.Platform.Common.ApplicationEventBus;

namespace Tavisca.Common.Plugins.Configuration
{
    public class ConfigurationUpdateEventHandler
    {
        private readonly IConfigurationStore _configurationStore;
        private readonly IApplicationEventBus _applicationEventBus;
        private const string EventName = "in-memory-consul-cache-refresh";

        public ConfigurationUpdateEventHandler(IConfigurationStore remoteConfigurationStore, ISensitiveDataProvider sensitiveDataProvider)
        {
            _configurationStore = new SecureConfigurationStore(remoteConfigurationStore, sensitiveDataProvider);
            _applicationEventBus = new InstanceEventBus();
        }

        public ConfigurationUpdateEventHandler(IConfigurationStore remoteConfigurationStore, IApplicationEventBus applicationEventBus, ISensitiveDataProvider sensitiveDataProvider)
        {
            _configurationStore = new SecureConfigurationStore(remoteConfigurationStore, sensitiveDataProvider);
            _applicationEventBus = applicationEventBus;
        }

        public void HandleEvent()
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    var kvStore = await _configurationStore.GetAllAsync();
                    if (kvStore == null || kvStore.Count == 0)
                        return;

                    LocalConfigurationRepository.ReplaceAll(new ConcurrentDictionary<string, string>(kvStore));
                }
                catch (Exception ex)
                {
                    Platform.Common.ExceptionPolicy.HandleException(ex, Constants.DefaultPolicy);
                }

            });
            task.ContinueWith(t =>
            {
                //raise an event on successful completion of above task.
                if (t.IsFaulted == false && t.IsCompleted)
                    _applicationEventBus.Notify(GetCacheRefreshedEvent());
            });

        }

        private ApplicationEvent GetCacheRefreshedEvent()
        {
            return new ApplicationEvent
            {
                Id = Guid.NewGuid().ToString(),
                Name = EventName,
                TimeStamp = DateTime.Now,
                Context = "cleared mem cache"
            };
        }
    }
}
