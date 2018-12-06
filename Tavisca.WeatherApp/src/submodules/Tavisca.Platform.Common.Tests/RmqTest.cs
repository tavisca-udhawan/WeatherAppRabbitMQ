using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Tavisca.Common.Plugins.Configuration;
using Tavisca.Common.Plugins.RabbitMq;
using Tavisca.Common.Plugins.RecyclableStreamPool;
using Tavisca.Platform.Common.ApplicationEventBus;
using Tavisca.Platform.Common.Plugins.Json;
using Tavisca.Platform.Common.ServiceBus;
using Xunit;

namespace Tavisca.Platform.Common.Tests
{

    public class RmqTest
    {
        [Fact(Skip = "added to test rmq connection ")]
        public async Task RmqTest_Connection()
        {
            var mockQueueSettings = new Mock<IQueueSettings>();
            //to test rmq connection and channel dispose add host, port, username password etc settings in moq setup
            mockQueueSettings.SetupSequence(x => x.GetReadQueueAsync("testSearch")).ReturnsAsync(new ReadQueue
            {

                Name = "testSearch",
                PrefetchSize = 1
            }).ReturnsAsync(new ReadQueue
            {

                Name = "testSearch",
                PrefetchSize = 10
            });

            mockQueueSettings.Setup(x => x.GetReadQueueAsync("testPrice")).ReturnsAsync(new ReadQueue
            {

                Name = "testPrice",
                PrefetchSize = 10
            });
            mockQueueSettings.Setup(x => x.GetWriteQueueAsync("testSearch")).ReturnsAsync(new WriteQueue()
            {

                Exchange = "testSearchExchange",
                RoutingKey = "testSearch"
            });
            var bus = new RmqBus(mockQueueSettings.Object, new JsonDotNetSerializer(new Newtonsoft.Json.JsonSerializer()), new RecyclableStreamPool());
            await bus.SubscribeAsync("testSearch", new TestSearchActor());
            await bus.SubscribeAsync("testPrice", new TestPriceActor());

            await bus.PublishAsync("testSearch", "test msg app one");
            Thread.Sleep(5000);
            new InstanceEventBus().Notify(new ApplicationEvent { Name = "in-memory-consul-cache-refresh", TimeStamp = DateTime.Now });
            Thread.Sleep(15000);
            await bus.PublishAsync("testSearch", "test msg app");
            Thread.Sleep(10000);
        }
    }

    public class TestSearchActor : IActor<string>
    {
        public Task NotifyAsync(IMessageWithRejectionSupport<string> msg)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }

        
    }

    public class TestPriceActor : IActor<TestPrice>
    {
        public Task NotifyAsync(IMessageWithRejectionSupport<TestPrice> msg)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {

        }

        
    }
    [Serializable]
    public class TestPrice
    {
        public const string Name = "TestPrice";
        public string Msg { get; set; }
    }
}
