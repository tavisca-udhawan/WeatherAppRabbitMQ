using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Tavisca.Common.Plugins.Configuration;
using Tavisca.Platform.Common.ApplicationEventBus;
using Xunit;
using Tavisca.Platform.Common.Configurations;

namespace Tavisca.Platform.Common.Tests
{
    /*
     * 1. on completion of local cache refresh raise event on local bus
     * 2. execute observers which have subscribed to those events.
     * 3. verify functionality of these observers
     */
    public class LocalEventBusTests
    {
        [Fact]
        public void VerifyEventNotification_OnSuccessfulCompletionOfCahceRefresh()
        {
            var eventData = new ApplicationEvent
            {
                Id = Guid.NewGuid().ToString(),
                Name = "test event",
                TimeStamp = DateTime.Now
            };
            var configStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();
            var mock = new Mock<IApplicationEventBus>();
            mock.Setup(x => x.Notify(It.Is<ApplicationEvent>(e => e.Name.Equals("in-memory-consul-cache-refresh", StringComparison.OrdinalIgnoreCase))));
            var dataSet = new Dictionary<string, string>();
            dataSet.Add("key", "value");
            configStoreMock.Setup(cs => cs.GetAllAsync()).ReturnsAsync(dataSet);

            var eventHandler = new ConfigurationUpdateEventHandler(configStoreMock.Object, mock.Object, sensitiveDataProviderMock.Object);
            var configUpdateObserver = new ConfigurationObserver(eventHandler);

            configUpdateObserver.Process(eventData);
          
            Thread.Sleep(5000);
           
            configUpdateObserver.Process(eventData);

            Thread.Sleep(5000);
            mock.Verify(x => x.Notify(It.Is<ApplicationEvent>(e => e.Name.Equals("in-memory-consul-cache-refresh", StringComparison.OrdinalIgnoreCase))), Times.Exactly(2));
        }

        [Fact]
        public void EventNotification_OnFailureOfTaskCompletion_NoeventHasRaised()
        {
            var id = Guid.NewGuid().ToString();
            var eventData = new ApplicationEvent
            {
                Id = id,
                Name = "test event",
                TimeStamp = DateTime.Now
            };
            var configStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();
            var mock = new Mock<IApplicationEventBus>();
            mock.Setup(x => x.Notify(It.Is<ApplicationEvent>(e => e.Name.Equals("in-memory-consul-cache-refresh", StringComparison.OrdinalIgnoreCase) && e.Id.Equals(id))));
    
            configStoreMock.Setup(cs => cs.GetAllAsync()).ThrowsAsync(new Exception());

            var eventHandler = new ConfigurationUpdateEventHandler(configStoreMock.Object, mock.Object, sensitiveDataProviderMock.Object);
            var configUpdateObserver = new ConfigurationObserver(eventHandler);

            configUpdateObserver.Process(eventData);

            Thread.Sleep(5000);
     
            mock.Verify(x => x.Notify(It.Is<ApplicationEvent>(e => e.Name.Equals("in-memory-consul-cache-refresh", StringComparison.OrdinalIgnoreCase) && e.Id.Equals(id))), Times.Never);
        }


        [Fact]
        public void EventExecutionVerification_OnSuccessfulCompletionOfCahceRefresh()
        {
            var localBus= new InstanceEventBus();
            localBus.Register("in-memory-consul-cache-refresh", new MockCacheRefreshObserver());
            var eventData = new ApplicationEvent
            {
                Id = Guid.NewGuid().ToString(),
                Name = "test event",
                TimeStamp = DateTime.Now
            };

            var configStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();

            var dataSet = new Dictionary<string, string>();
            dataSet.Add("key", "value");
            configStoreMock.Setup(cs => cs.GetAllAsync()).ReturnsAsync(dataSet);

            var eventHandler = new ConfigurationUpdateEventHandler(configStoreMock.Object, localBus, sensitiveDataProviderMock.Object);

            var configUpdateObserver = new ConfigurationObserver(eventHandler);

            configUpdateObserver.Process(eventData);

            Thread.Sleep(5000);
            Assert.Equal(1, MockCacheRefreshObserver.Count);


            configUpdateObserver.Process(eventData);

            Thread.Sleep(5000);

            Assert.Equal(2, MockCacheRefreshObserver.Count);

        }
    }

    public class MockCacheRefreshObserver : ApplicationEventObserver
    {
        public static int Count = 0;
        public override void Process(ApplicationEvent msg)
        {
            base.Process(msg);
            Count++;
        }
    }
}
