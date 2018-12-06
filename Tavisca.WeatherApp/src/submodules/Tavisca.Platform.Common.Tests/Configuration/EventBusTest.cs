using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Moq;
using System.Threading;
using Tavisca.Common.Plugins.Configuration;
using Tavisca.Platform.Common.ApplicationEventBus;

namespace Tavisca.Platform.Common.Tests
{
    [Ignore]
    [TestClass]
    public class EventBusTest
    {

        [TestMethod]
        public async Task RaiseAndListenToEvents_Basic_ValidCase()
        {
            var bus = new RXEventChannel();
            var mockEventObserver = new MOQObserver();
            var applicationEvent = new ApplicationEvent()
            {
                Context = "Hi",
                Name = "Invalidate-Cache",
                Id = Guid.NewGuid().ToString(),
                Publisher = "Tushar",
                TimeStamp = DateTime.UtcNow
            };

            var eventObserver = bus.GetChannel("Invalidate-Cache");
            eventObserver.Subscribe(mockEventObserver);

            var provider = new SignalREventChannel(bus, new Tavisca.Common.Plugins.Configuration.JsonSerializer());

            Thread.Sleep(2000);

            provider.Notify(applicationEvent);

            Thread.Sleep(5000);

            Assert.AreEqual(MOQObserver.EventId, applicationEvent.Id);
        }

        [TestMethod]
        public async Task RaiseAndListenToEvents_MultipleEvents_ValidCase()
        {
            var bus = new RXEventChannel();
            var mockEventObserver = new MOQObserver();
            var applicationEvent = new ApplicationEvent()
            {
                Context = "Hi",
                Name = "Invalidate-Cache",
                Id = Guid.NewGuid().ToString(),
                Publisher = "Tushar",
                TimeStamp = DateTime.UtcNow
            };

            var eventObserver = bus.GetChannel("Invalidate-Cache");
            eventObserver.Subscribe(mockEventObserver);

            var provider = new SignalREventChannel(bus, new Tavisca.Common.Plugins.Configuration.JsonSerializer());

            Thread.Sleep(2000);

            provider.Notify(applicationEvent);

            Thread.Sleep(2000);

            provider.Notify(applicationEvent);

            Thread.Sleep(5000);

            Assert.AreEqual(mockEventObserver.CallCount, 2);
        }

        [TestMethod]
        public async Task RaiseAndListenToEvents_MultipleListners_ValidCase()
        {
            var bus = new RXEventChannel();
            var mockEventObserver = new MOQObserver();
            var applicationEvent = new ApplicationEvent()
            {
                Context = "Hi",
                Name = "Invalidate-Cache",
                Id = Guid.NewGuid().ToString(),
                Publisher = "Tushar",
                TimeStamp = DateTime.UtcNow
            };

            var eventObserver1 = bus.GetChannel("Invalidate-Cache");
            eventObserver1.Subscribe(mockEventObserver);

            var eventObserver2 = bus.GetChannel("Invalidate-Cache");
            eventObserver2.Subscribe(mockEventObserver);

            var provider = new SignalREventChannel(bus, new Tavisca.Common.Plugins.Configuration.JsonSerializer());

            Thread.Sleep(2000);

            provider.Notify(applicationEvent);

            Thread.Sleep(5000);

            Assert.AreEqual(mockEventObserver.CallCount, 2);
        }

        [TestMethod]
        public async Task RaiseAndListenToEvents_ApplicationBus_ValidCase()
        {
            var bus = new Tavisca.Common.Plugins.Configuration.ApplicationEventBus();
            var mockEventObserver = new MOQObserver();
            var applicationEvent = new ApplicationEvent()
            {
                Context = "Hi",
                Name = "Invalidate-Cache",
                Id = Guid.NewGuid().ToString(),
                Publisher = "Tushar",
                TimeStamp = DateTime.UtcNow
            };

            bus.Register("Invalidate-Cache", mockEventObserver);

            Thread.Sleep(2000);

            bus.Notify(applicationEvent);

            Thread.Sleep(5000);

            Assert.AreEqual(MOQObserver.EventId, applicationEvent.Id);
        }
    }

    public class MOQObserver : ApplicationEventObserver
    {
        public static string EventId;

        public int CallCount;

        public override void Process(ApplicationEvent msg)
        {
            base.Process(msg);

            EventId = msg.Id;

            CallCount++;

        }

    }
}
