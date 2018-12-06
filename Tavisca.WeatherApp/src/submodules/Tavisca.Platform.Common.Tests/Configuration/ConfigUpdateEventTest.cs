using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Moq;
using System.Threading;
using Tavisca.Common.Plugins.Configuration;
using Tavisca.Platform.Common.ApplicationEventBus;
using System.Collections.Generic;
using Tavisca.Common.Plugins.EnterpriseLibrary;
using Tavisca.Platform.Common.Configurations;

namespace Tavisca.Platform.Common.Tests
{
    [Ignore]
    [TestClass]
    public class ConfigUpdateEventTest
    {

        [TestMethod]
        public async Task ConfigUpdateEvent_ObserverCall_ValidCase()
        {

            var bus = new RXEventChannel();
            var configStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();
            var mock = new Mock<IApplicationEventBus>();
            mock.Setup(x => x.Notify(It.IsAny<ApplicationEvent>()));
            var dataSet = new Dictionary<string, string>();
            dataSet.Add("key", "value");
            configStoreMock.Setup(cs => cs.GetAllAsync()).ReturnsAsync(dataSet);

            var eventHandler = new ConfigurationUpdateEventHandler(configStoreMock.Object, mock.Object, sensitiveDataProviderMock.Object);
            var confifUpdateObserver = new ConfigurationObserver(eventHandler);

            var applicationEvent = new ApplicationEvent()
            {
                Context = "config-update",
                Name = "config-update",
                Id = Guid.NewGuid().ToString(),
                Publisher = "Tushar",
                TimeStamp = DateTime.UtcNow
            };

            var eventObserver = bus.GetChannel("config-update");
            eventObserver.Subscribe(confifUpdateObserver);

            var provider = new SignalREventChannel(bus, new Tavisca.Common.Plugins.Configuration.JsonSerializer());

            Thread.Sleep(2000);

            provider.Notify(applicationEvent);

            Thread.Sleep(5000);

            Assert.IsTrue(LocalConfigurationRepository.IsKeyPresent("key"));
        }

        [TestMethod]
        public async Task ConfigUpdateEvent_VerifyEventInvokation_OnSuccessfulCompletion()
        {

            var bus = new RXEventChannel();
            var configStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();
            var mock = new Mock<IApplicationEventBus>();
            mock.Setup(x => x.Notify(It.Is<ApplicationEvent>(e=>e.Name.Equals("in-memory-consul-cache-refresh", StringComparison.OrdinalIgnoreCase))));
            var dataSet = new Dictionary<string, string>();
            dataSet.Add("key", "value");
            configStoreMock.Setup(cs => cs.GetAllAsync()).ReturnsAsync(dataSet);

            var eventHandler = new ConfigurationUpdateEventHandler(configStoreMock.Object, mock.Object, sensitiveDataProviderMock.Object);
            var confifUpdateObserver = new ConfigurationObserver(eventHandler);

            var applicationEvent = new ApplicationEvent()
            {
                Context = "config-update",
                Name = "config-update",
                Id = Guid.NewGuid().ToString(),
                Publisher = "Tushar",
                TimeStamp = DateTime.UtcNow
            };

            var eventObserver = bus.GetChannel("config-update");
            eventObserver.Subscribe(confifUpdateObserver);

            var provider = new SignalREventChannel(bus, new Tavisca.Common.Plugins.Configuration.JsonSerializer());

            Thread.Sleep(2000);

            provider.Notify(applicationEvent);

            Thread.Sleep(6000);

            Assert.IsTrue(LocalConfigurationRepository.IsKeyPresent("key"));
            //verify if the event has been raised on completion of config update

            mock.Verify(x=>x.Notify(It.Is<ApplicationEvent>(e => e.Name.Equals("in-memory-consul-cache-refresh", StringComparison.OrdinalIgnoreCase))));
        }
    }
}
