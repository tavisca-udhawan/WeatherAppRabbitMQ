using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Moq;
using System.Threading;
using System.Collections.Generic;
using Tavisca.Common.Plugins.Configuration;
using Tavisca.Platform.Common.Configurations;

namespace Tavisca.Platform.Common.Tests
{
    [TestClass]
    public class CachedConfigurationStoreTest
    {

        [TestMethod]
        public async Task CachedConfiguration_NoDataInCahce_RetrunFromConsul()
        {

            var consulStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();

            var provider1 = new CachedConfigurationStore(consulStoreMock.Object, sensitiveDataProviderMock.Object);

            consulStoreMock.Setup(store => store.GetAsync("global", "content_service", "invalildSection", "invalidKey")).ReturnsAsync("Hello");

            var value = await provider1.GetAsync("global", "content_service", "invalildSection", "invalidKey");            

            Assert.AreEqual(value, "Hello");
        }


        [TestMethod]
        public async Task CachedConfiguration_NoDataInCahce_RetrunFromConsul_UpdateCache()
        {

            var consulStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();

            var provider1 = new CachedConfigurationStore(consulStoreMock.Object, sensitiveDataProviderMock.Object);

            consulStoreMock.Setup(store => store.GetAsync("global", "content_service", "invalildSection", "invalidKey")).ReturnsAsync("Hello");

            var value = await provider1.GetAsync("global", "content_service", "invalildSection", "invalidKey");

            consulStoreMock.Setup(store => store.GetAsync("global", "content_service", "invalildSection", "invalidKey")).ReturnsAsync("Mello");

            value = await provider1.GetAsync("global", "content_service", "invalildSection", "invalidKey");

            Assert.AreEqual(value, "Hello");
            Assert.AreNotEqual(value, "Mello");
        }

    }


}
