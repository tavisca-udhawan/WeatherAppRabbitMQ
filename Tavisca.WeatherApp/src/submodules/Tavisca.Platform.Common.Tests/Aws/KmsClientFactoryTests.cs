using Moq;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.Aws.KMS;
using Tavisca.Platform.Common.ApplicationEventBus;
using Tavisca.Platform.Common.Configurations;
using Xunit;
using Tavisca.Common.Plugins.Configuration;
using System.Threading;

namespace Tavisca.Platform.Common.Tests.Aws
{
    public class KmsClientFactoryTests
    {
        static IApplicationEventBus eventBus = new InstanceEventBus();

        [Fact]
        public async Task GetClientAsync_ShouldReturnClient()
        {
            var clientFactory = new KmsClientFactory(null);

            var settings = GetKmsSettings("us-east-1");
            var clientA = await clientFactory.GetClientAsync(settings);
            Assert.NotNull(clientA);

            //Should return same client .i.e returns client from cache
            var clientB = await clientFactory.GetClientAsync(settings);
            Assert.NotNull(clientB);
            Assert.Equal(clientA, clientB);

            //Should return different client
            settings = GetKmsSettings("us-east-2");
            var clientC = await clientFactory.GetClientAsync(settings);
            Assert.NotNull(clientA);
            Assert.NotEqual(clientA, clientC);
        }

        [Fact]
        public void GetCient_ShouldReturnClient()
        {
            var clientFactory = new KmsClientFactory(null);

            var settings = GetKmsSettings("us-east-1");
            var clientA = clientFactory.GetClient(settings);
            Assert.NotNull(clientA);

            //Should return same client .i.e returns client from cache
            var clientB = clientFactory.GetClient(settings);
            Assert.NotNull(clientB);
            Assert.Equal(clientA, clientB);

            //Should return different client
            settings = GetKmsSettings("us-east-2");
            var clientC = clientFactory.GetClient(settings);
            Assert.NotNull(clientA);
            Assert.NotEqual(clientA, clientC);
        }

        [Fact]
        public async Task GetTenantSpecificClientAsync_ShouldReturnClient()
        {
            var tenant = "demo";
            var config = new Mock<IConfigurationProvider>();
            config.Setup(x => x.GetTenantConfigurationAsNameValueCollectionAsync(tenant, 
                It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(GetNamevalueCollection("us-east-1"));

            var clientFactory = new KmsClientFactory(config.Object);

            var clientA = await clientFactory.GetTenantSpecificClientAsync(tenant);
            Assert.NotNull(clientA);

            //Should return same client .i.e returns client from cache
            var clientB = await clientFactory.GetTenantSpecificClientAsync(tenant);
            Assert.NotNull(clientB);
            Assert.Equal(clientA, clientB);
        }

        [Fact]
        public void GetTenantSpecificClient_ShouldReturnClient()
        {
            var tenant = "demo";
            var config = new Mock<IConfigurationProvider>();
            config.Setup(x => x.GetTenantConfigurationAsNameValueCollection(tenant,
                It.IsAny<string>(), It.IsAny<string>())).Returns(GetNamevalueCollection("us-east-1"));

            var clientFactory = new KmsClientFactory(config.Object);

            var clientA = clientFactory.GetTenantSpecificClient(tenant);
            Assert.NotNull(clientA);

            //Should return same client .i.e returns client from cache
            var clientB = clientFactory.GetTenantSpecificClient(tenant);
            Assert.NotNull(clientB);
            Assert.Equal(clientA, clientB);

        }

        [Fact]
        public async Task GetGlobalClientAsync_ShouldReturnClient()
        {
            var config = new Mock<IConfigurationProvider>();
            config.Setup(x => x.GetGlobalConfigurationAsNameValueCollectionAsync(It.IsAny<string>(), 
                 It.IsAny<string>())).ReturnsAsync(GetNamevalueCollection("us-east-1"));

            var clientFactory = new KmsClientFactory(config.Object);

            var client = await clientFactory.GetGlobalClientAsync();
            Assert.NotNull(client);

            //Should return same client .i.e returns client from cache
            var client2 = await clientFactory.GetGlobalClientAsync();
            Assert.NotNull(client);
            Assert.Equal(client, client2);
        }

        [Fact]
        public void GetGlobalClient_ShouldReturnClient()
        {
            var config = new Mock<IConfigurationProvider>();
            config.Setup(x => x.GetGlobalConfigurationAsNameValueCollection(It.IsAny<string>(), 
                It.IsAny<string>())).Returns(GetNamevalueCollection("us-east-1"));

            var clientFactory = new KmsClientFactory(config.Object);

            var client = clientFactory.GetGlobalClient();
            Assert.NotNull(client);

            //Should return same client .i.e returns client from cache
            var client2 = clientFactory.GetGlobalClient();
            Assert.NotNull(client);
            Assert.Equal(client, client2);
        }

        //Test if clients is cleared on local inmemory consul cache refresh event 
        //create client for TenantA
        //Update settings for TenantA in mock consul
        //Now Process will be called and it will clear existing client and and will create new client with updated settings
        //Now get the client for TenantA, now it should return different client instance8
        [Fact]
        public async Task Process_ApplicationEvent_ShouldRemoveUnusedClients()
        {
            var tenant = "demo_tenant";
            var config = new Mock<IConfigurationProvider>();
            config.Setup(x => x.GetTenantConfigurationAsNameValueCollectionAsync(tenant, 
                It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(GetNamevalueCollection("us-west-1"));

            //Get client first time, should create new client and return
            var clientFactory = new KmsClientFactory(config.Object);
            var clientA = await clientFactory.GetTenantSpecificClientAsync(tenant);
            Assert.NotNull(clientA);

            //Get the client second time, should return client from cache
            var clientB = await clientFactory.GetTenantSpecificClientAsync(tenant);
            Assert.NotNull(clientB);
            Assert.Equal(clientA, clientB);

            //Update consul settings
            config.Setup(x => x.GetTenantConfigurationAsNameValueCollectionAsync(tenant,
                It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(GetNamevalueCollection("us-west-2"));

            var eventData = new ApplicationEvent
            {
                Id = Guid.NewGuid().ToString(),
                Name = "in-memory-consul-cache-refresh",
                TimeStamp = DateTime.Now
            };

            //Process is invoked on consule event and clears unused clients 
            clientFactory.Process(eventData);
            Thread.Sleep(5000);
            
            // Get the client third time, this should return different client instance for same tenant as settings in consul is updated
            var updatedClient = await clientFactory.GetTenantSpecificClientAsync(tenant);
            Assert.NotNull(updatedClient);
            Assert.NotEqual(updatedClient, clientA);
        }

        private static KmsSettings GetKmsSettings(string region)
        {
            var settingsNvc = new NameValueCollection
            {
                { "region", region },
                { "access_key", "access_key" },
                { "secret_key", "secret_key" }
            };

           return KmsSettings.Load(settingsNvc);
        }

        private static NameValueCollection GetNamevalueCollection(string region)
        {
            var settingsNvc = new NameValueCollection
            {
                { "region", region },
                { "access_key", "access_key" },
                { "secret_key", "secret_key" }
            };

            return settingsNvc;
        }
    }
}