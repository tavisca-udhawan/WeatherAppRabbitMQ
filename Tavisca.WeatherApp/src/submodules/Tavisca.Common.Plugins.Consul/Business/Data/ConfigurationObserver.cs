using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.ApplicationEventBus;

namespace Tavisca.Common.Plugins.Configuration
{
    public class ConfigurationObserver : ApplicationEventObserver
    {
        private readonly ConfigurationUpdateEventHandler _eventHandler;
        public ConfigurationObserver(ConfigurationUpdateEventHandler eventHandler)
        {
            _eventHandler = eventHandler;
        }

        public ConfigurationObserver(IConfigurationStore remoteConfigurationStore)
        {
            _eventHandler = new ConfigurationUpdateEventHandler(remoteConfigurationStore, null);
        }

        public ConfigurationObserver(IConfigurationStore remoteConfigurationStore, ISensitiveDataProvider sensitiveDataProvider)
        {
            _eventHandler = new ConfigurationUpdateEventHandler(remoteConfigurationStore, sensitiveDataProvider);
        }

        public override void Process(ApplicationEvent msg)
        {
            base.Process(msg);
            _eventHandler.HandleEvent();
        }
    }
}
