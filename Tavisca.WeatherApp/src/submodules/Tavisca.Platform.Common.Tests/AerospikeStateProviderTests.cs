using Moq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.Aerospike;
using Tavisca.Common.Plugins.BinarySerializer;
using Tavisca.Common.Plugins.RecyclableStreamPool;
using Tavisca.Common.Plugins.SessionStore;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.Serialization;
using Tavisca.Platform.Common.SessionStore;
using Xunit;

namespace Tavisca.Platform.Common.Tests
{


    public class AerospikeStateProviderTests
    {

        private const string ConfigurationSection = "framework_settings";

        private const string ConfigurationKey = "data_store";

        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task SingleItemPutTest()
        {
            var mockData = new SampleClass() { Id = 123, Data = "Kratos" };
            var key = Guid.NewGuid().ToString();
            var category = "spartan";

            var aerospikeStateProvider = new AerospikeStateProvider(new AerospikeClientFactory(), GetMockConfigurationProvider(), GetMockSessionStoreSerializerFactory().Object, new RecyclableStreamPool());
            await aerospikeStateProvider.AddAsync(new ItemKey(category, key), mockData);

            var item = await aerospikeStateProvider.GetAsync<SampleClass>(new ItemKey(category, key));
            Assert.Equal(mockData.Data, item.Data);
        }


        [Fact(Skip ="Call aerospike host which is not available")]
        public async Task RemoveItemTest()
        {
            var mockData = new SampleClass() { Id = 123, Data = "Kratos" };
            var key = Guid.NewGuid().ToString();
            var category = "spartan";

            var aerospikeStateProvider = new AerospikeStateProvider(new AerospikeClientFactory(), GetMockConfigurationProvider(), GetMockSessionStoreSerializerFactory().Object, new RecyclableStreamPool());
            await aerospikeStateProvider.AddAsync(new ItemKey(category, key), mockData);

            var item = await aerospikeStateProvider.GetAsync<SampleClass>(new ItemKey(category, key));
            Assert.Equal(mockData.Data, item.Data);

            await aerospikeStateProvider.RemoveAsync(new ItemKey(category, key));

            var RemovedItem = await aerospikeStateProvider.GetAsync<SampleClass>(new ItemKey(category, key));
            Assert.Null(RemovedItem);
        }


        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task PutByteArrayTest()
        {
            var mockData = new SampleClass() { Id = 123, Data = "Kratos" };

            var mockByte = mockData.ConvertToByte();
            var key = Guid.NewGuid().ToString();
            var category = "spartan";

            var serializeMock = GetMockSessionStoreSerializerFactory();
            var aerospikeStateProvider = new AerospikeStateProvider(new AerospikeClientFactory(), GetMockConfigurationProvider(), serializeMock.Object, new RecyclableStreamPool());
            await aerospikeStateProvider.AddAsync(new ItemKey(category, key), mockByte);



            var data = await aerospikeStateProvider.GetAsync<Byte[]>(new ItemKey(category, key));
            var item = Deserialize(data);

            Assert.Equal(mockData.Data, item.Data);

            serializeMock.Verify(x => x.Create(It.IsAny<Type>()), Times.Exactly(0));
        }

        private SampleClass Deserialize(byte[] data)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                var sampleClass = bf.Deserialize(ms);
                return (SampleClass)sampleClass;
            }
        }

        [Fact(Skip = "Call aerospike host which is not available")]
        public async Task MultipleItemPutTest()
        {

            var key = Guid.NewGuid().ToString();
            var key1 = Guid.NewGuid().ToString();
            var category = "spartan";

            var mockData = new List<DataItem<SampleClass>>() { { new DataItem<SampleClass>(new ItemKey(category, key), new SampleClass() { Id = 12345, Data = "first" }) }, { new DataItem<SampleClass>(new ItemKey(category, key1), new SampleClass() { Id = 12345, Data = "Second" }) } };

            var aerospikeStateProvider = new AerospikeStateProvider(new AerospikeClientFactory(), GetMockConfigurationProvider(), GetMockSessionStoreSerializerFactory().Object, new RecyclableStreamPool());

            var dataItems = new DataItem<SampleClass>[2];
            await aerospikeStateProvider.AddMultipleAsync<SampleClass>(mockData.ToArray());

            var itemKeys = new ItemKey[2];
            itemKeys[0] = new ItemKey(category, key);
            itemKeys[1] = new ItemKey(category, key1);
            var item = await aerospikeStateProvider.GetAllAsync<SampleClass>(itemKeys);
            Assert.Equal(mockData[0].Value.Data, item[0].Value.Data);
            Assert.Equal(mockData[1].Value.Data, item[1].Value.Data);
        }





        private Mock<ISerializerFactory> GetMockSessionStoreSerializerFactory()
        {
            var mockSessionStoreSerializerFactory = new Mock<ISerializerFactory>();
            mockSessionStoreSerializerFactory.Setup(x => x.Create(It.IsAny<Type>())).Returns(new BinarySerializer());
            return mockSessionStoreSerializerFactory;
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
            mock.Setup(x => x.GetGlobalConfigurationAsync<AerospikeSettings>(Keystore.AerospikeKeys.SettingsSection, Keystore.AerospikeKeys.StateSettings))
                .ReturnsAsync(new AerospikeSettings { Host = host, Port = 3000, Namespace = ns });
            return mock.Object;
        }

    }
}
