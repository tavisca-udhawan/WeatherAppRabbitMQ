using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.Aerospike;
using Xunit;

namespace Tavisca.Platform.Common.Tests
{
    public class AerospikeClientFactoryTest
    {
        /*
         * 1. to test if settings is requested then , output is returned for it
         * 2. if connected status of client is false , then it should create new client
         * 3. if settings for new set of host and port is requested then ,new setting is returned .
         */
        private static IAerospikeClientFactory _aerospikeClientFactory = new AerospikeClientFactory();

        [Fact]
        public void ReturnClientForRequestedSettings()
        {
            var host1 = "172.16.3.91";
            var port1 = 3000;
            var client1 =  _aerospikeClientFactory.GetClient(host1, port1, new List<string>());
            Assert.NotNull(client1);
            Assert.NotEmpty(client1.Nodes);
            var host = client1.Nodes[0].Host.name;
            var port= client1.Nodes[0].Host.port;
            Assert.Equal(host1,host);
            Assert.Equal(port1, port);
        }

        //[Fact]
        //public void Return_DifferentClients_IfDiffrentSettingsAreRequested()
        //{
        //    var host1 = "172.16.3.91";
        //    var port1 = 3000;
        //    var client1 =  _aerospikeClientFactory.GetClient(host1, port1);
        //    Assert.NotNull(client1);
        //    Assert.NotEmpty(client1.Nodes);
        //    var clientHost1 = client1.Nodes[0].Host.name;
        //    var clientPort1 = client1.Nodes[0].Host.port;

        //    Assert.Equal(host1, clientHost1);
        //    Assert.Equal(port1, clientPort1);

            //var host2 = "10.0.8.249";
            //var port2 = 3000;
            //var client2 =  _aerospikeClientFactory.GetClient(host2, port2, new List<string>());
            //Assert.NotNull(client2);
        //    var host2 = "10.0.7.69";
        //    var port2 = 3000;
        //    var client2 =  _aerospikeClientFactory.GetClient(host2, port2);
        //    Assert.NotNull(client2);

        //    Assert.NotEmpty(client2.Nodes);
        //    var clientHost2 = client2.Nodes[0].Host.name;
        //    var clientPort2 = client2.Nodes[0].Host.port;

        //    Assert.Equal(host2, clientHost2);
        //    Assert.Equal(port2,clientPort2);

         
        //}
    }
}
