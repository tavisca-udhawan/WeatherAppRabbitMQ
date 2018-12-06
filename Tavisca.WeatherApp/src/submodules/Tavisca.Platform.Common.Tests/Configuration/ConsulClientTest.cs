using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Moq;
using System.Threading;
using System.Collections.Generic;
using Consul;
using System.Text;

namespace Tavisca.Platform.Common.Tests
{
    
    [Ignore]
    [TestClass]
    public class ConsulClientTest
    {

        [TestMethod]
        public async Task FireEventOnConsul_Valid()
        {
            var connectionString = "http://localhost:8500";
            var eventName = "application-event";
            var payloadString = "test load";
            var utf8WithoutBom = new UTF8Encoding(false);
            var plainTextBytes = utf8WithoutBom.GetBytes(payloadString);

            var instance = new ConsulClient(new ConsulClientConfiguration() { Address = new Uri(connectionString) });
            var result = await instance.Event.Fire(new UserEvent { Name = eventName, Payload = plainTextBytes, });

            Assert.IsNotNull(result);

        }

    }


}
