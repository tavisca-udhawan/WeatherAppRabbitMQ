using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tavisca.Common.Plugins.Configuration;
using System.Threading;

namespace Tavisca.Platform.Common.Tests.Configuration
{
    [Ignore]
    [TestClass]
    public class EventBusConnectionTest
    {
        [TestMethod]
        public void ConnectionCheck()
        {
            var channel = new SignalREventChannel();
            Thread.Sleep(5000);
        }
    }
}
