using Moq;
using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.SessionStore;
using Tavisca.Platform.Common.Configurations;
using Xunit;
using System.Linq;
using Tavisca.Common.Plugins.Aerospike;
using Tavisca.Platform.Common.SessionStore;
using Tavisca.Common.Plugins.RecyclableStreamPool;
using Tavisca.Platform.Common.Serialization;
using Tavisca.Common.Plugins.BinarySerializer;
using Tavisca.Platform.Common.Containers;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Tavisca.Platform.Common.Tests
{
    public class SessionStoreTests
    {
        private const string ConfigurationSection = "framework_settings";
        private const string ConfigurationKey = "data_store";
        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task TestSingleItemOperation()
        {
            var mockConfigProvider = GetMockConfigurationProvider();
            var mockSessionStoreSerializerFactory = GetMockSessionStoreSerializerFactory();

            var mockSessionProviderFactory = GetSessionProviderFactory(mockSessionStoreSerializerFactory, mockConfigProvider);
            var sessionStore = new Tavisca.Common.Plugins.SessionStore.SessionStore(mockSessionProviderFactory);

            var mockData = new SampleClass() { Id = 123, Data = "Kratos" };
            var key = Guid.NewGuid().ToString();
            var category = "spartan";
            await sessionStore.AddAsync(new DataItem<SampleClass>(new ItemKey(category, key), mockData));
            var readData = await sessionStore.GetAsync<SampleClass>(new ItemKey(category, key), CancellationToken.None);

            Assert.NotNull(readData);
            Assert.Equal(mockData.Id, readData.Id);
            Assert.Equal(mockData.Data, readData.Data);
        }

        private ISerializerFactory GetMockSessionStoreSerializerFactory()
        {
            var mockSessionStoreSerializerFactory = new Mock<ISerializerFactory>();
            mockSessionStoreSerializerFactory.Setup(x => x.Create(It.IsAny<Type>())).Returns(new BinarySerializer());
            return mockSessionStoreSerializerFactory.Object;
        }

        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task TestMultiItemOperation()
        {
            var mockConfigProvider = GetMockConfigurationProvider();
            var mockSessionStoreSerializerFactory = GetMockSessionStoreSerializerFactory();


            var mockSessionProviderFactory = GetSessionProviderFactory(mockSessionStoreSerializerFactory, mockConfigProvider);
            var sessionStore = new Tavisca.Common.Plugins.SessionStore.SessionStore(mockSessionProviderFactory);

            var key = Guid.NewGuid().ToString();
            var category_base = "spartan";
            var itemCount = 100;
            var categories = new string[itemCount];
            for (var i = 0; i < itemCount; i++)
                categories[i] = category_base + i;
            var id = 1;
            var dataItems = categories
                .Select(x => new DataItem<SampleClass>(new ItemKey(x, key), new SampleClass() { Id = id++, Data = id.ToString() })).ToArray();

            await sessionStore.AddMultipleAsync(dataItems);

            var itemKeys = categories.Select(x => new ItemKey(x, key)).ToArray();
            var readItems = await sessionStore.GetMultipleAsync<SampleClass>(itemKeys);

            foreach (var dataItem in dataItems)
            {
                var match = readItems.SingleOrDefault(x => x.Key.Equals(dataItem.Key));
                Assert.NotNull(match);
                Assert.Equal(dataItem.Value.Id, match.Value.Id);
                Assert.Equal(dataItem.Value.Data, match.Value.Data);
            }
        }

        private IConfigurationProvider GetMockConfigurationProvider()
        {
            var host = ConfigurationManager.AppSettings["AerospikeHost"];
            var ns = ConfigurationManager.AppSettings["SessionNamespace"];
            var config = new ExternalConfiguration()
            {
                ExpiryInSeconds = 9999,
                MaxItemsPerAsyncQueue = 10
            };

            var mock = new Mock<IConfigurationProvider>();

            mock.Setup(x => x.GetGlobalConfigurationAsync<ExternalConfiguration>(ConfigurationSection, ConfigurationKey)).ReturnsAsync(config);
            mock.Setup(x => x.GetGlobalConfigurationAsync<AerospikeSettings>(Keystore.AerospikeKeys.SettingsSection, Keystore.AerospikeKeys.SessionProviderSettings))
                .ReturnsAsync(new AerospikeSettings { Host = host, Port = 3000, Namespace = ns });
            return mock.Object;
        }


        private ISessionProviderFactory GetSessionProviderFactory(ISerializerFactory serializerFactory, IConfigurationProvider configurationProvider)
        {
            var sessionProviderFactory = new Mock<ISessionProviderFactory>();
            sessionProviderFactory.Setup(x => x.GetSessionProvider()).Returns(new AeroSpikeSessionProvider(new AerospikeClientFactory(), configurationProvider, serializerFactory, new RecyclableStreamPool()));
            return sessionProviderFactory.Object;
        }
    }

    [Serializable]
    public class SampleClass
    {
        public int Id { get; set; }
        public string Data { get; set; }

        public byte[] ConvertToByte()
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, this);
                return ms.ToArray();
            }
        }
    }
}
