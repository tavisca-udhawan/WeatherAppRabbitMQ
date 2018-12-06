using Moq;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using Tavisca.Common.Plugins.Configuration;
using Tavisca.Platform.Common.Configurations;

namespace Tavisca.Platform.Common.Tests.Core.Configuration
{
    public class SecureConfigurationStoreTests
    {
        [Fact]
        public void Get_Without_SecureData()
        {
            var mockConsulStore = new Mock<IConfigurationStore>();
            mockConsulStore.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns("test");

            var mockSensitiveDataProvider = new Mock<ISensitiveDataProvider>();
            mockSensitiveDataProvider.Setup(x => x.GetValuesAsync(It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(new Dictionary<string, string>());

            var secureConfigurationStore = new SecureConfigurationStore(mockConsulStore.Object, mockSensitiveDataProvider.Object);

            var result = secureConfigurationStore.Get("global", "test_app", "test_section", "test_key");

            Assert.Equal("test", result);
            mockConsulStore.Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockSensitiveDataProvider.Verify(x => x.GetValuesAsync(It.IsAny<List<string>>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public void Get_With_SecureData()
        {
            var mockConsulStore = new Mock<IConfigurationStore>();
            mockConsulStore.Setup(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns("{\"test\":\"[#test_one#]\"}");

            var sensitiveData = new Dictionary<string, string>();
            sensitiveData.Add("test_one", "1");
            var mockSensitiveDataProvider = new Mock<ISensitiveDataProvider>();
            mockSensitiveDataProvider.Setup(x => x.GetValuesAsync(It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(sensitiveData);

            var secureConfigurationStore = new SecureConfigurationStore(mockConsulStore.Object, mockSensitiveDataProvider.Object);

            var result = secureConfigurationStore.Get("global", "test_app", "test_section", "test_key");

            Assert.Equal("{\"test\":\"1\"}", result);
            mockConsulStore.Verify(x => x.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockSensitiveDataProvider.Verify(x => x.GetValuesAsync(It.IsAny<List<string>>(), It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task GetAsync_Without_SecureData()
        {
            var mockConsulStore = new Mock<IConfigurationStore>();
            mockConsulStore.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("test");

            var mockSensitiveDataProvider = new Mock<ISensitiveDataProvider>();
            mockSensitiveDataProvider.Setup(x => x.GetValuesAsync(It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(new Dictionary<string, string>());

            var secureConfigurationStore = new SecureConfigurationStore(mockConsulStore.Object, mockSensitiveDataProvider.Object);

            var result = await secureConfigurationStore.GetAsync("global", "test_app", "test_section", "test_key");

            Assert.Equal("test", result);
            mockConsulStore.Verify(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockSensitiveDataProvider.Verify(x => x.GetValuesAsync(It.IsAny<List<string>>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task GetAsync_With_SecureData()
        {
            var mockConsulStore = new Mock<IConfigurationStore>();
            mockConsulStore.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync("{\"test\":\"[#test_one#]\"}");

            var sensitiveData = new Dictionary<string, string>();
            sensitiveData.Add("test_one", "1");
            var mockSensitiveDataProvider = new Mock<ISensitiveDataProvider>();
            mockSensitiveDataProvider.Setup(x => x.GetValuesAsync(It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(sensitiveData);

            var secureConfigurationStore = new SecureConfigurationStore(mockConsulStore.Object, mockSensitiveDataProvider.Object);

            var result = await secureConfigurationStore.GetAsync("global", "test_app", "test_section", "test_key");

            Assert.Equal("{\"test\":\"1\"}", result);
            mockConsulStore.Verify(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            mockSensitiveDataProvider.Verify(x => x.GetValuesAsync(It.IsAny<List<string>>(), It.IsAny<bool>()), Times.Once);
        }        

        [Fact]
        public async Task GetAllAsync_Without_SecureData()
        {
            var consulConfiguration = new Dictionary<string, string>();
            consulConfiguration.Add("test", "test");
            var mockConsulStore = new Mock<IConfigurationStore>();
            mockConsulStore.Setup(x => x.GetAllAsync()).ReturnsAsync(consulConfiguration);

            var mockSensitiveDataProvider = new Mock<ISensitiveDataProvider>();
            mockSensitiveDataProvider.Setup(x => x.GetValuesAsync(It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(new Dictionary<string, string>());

            var secureConfigurationStore = new SecureConfigurationStore(mockConsulStore.Object, mockSensitiveDataProvider.Object);

            var result = await secureConfigurationStore.GetAllAsync();

            Assert.Equal(consulConfiguration.Count, result.Count);
            mockConsulStore.Verify(x => x.GetAllAsync(), Times.Once);
            mockSensitiveDataProvider.Verify(x => x.GetValuesAsync(It.IsAny<List<string>>(), It.IsAny<bool>()), Times.Never);
        }

        [Fact]
        public async Task GetAllAsync_With_SecureData()
        {
            var consulConfiguration = new Dictionary<string, string>();
            consulConfiguration.Add("test", "[#test_one#]");
            var mockConsulStore = new Mock<IConfigurationStore>();
            mockConsulStore.Setup(x => x.GetAllAsync()).ReturnsAsync(consulConfiguration);

            var sensitiveData = new Dictionary<string, string>();
            sensitiveData.Add("test_one", "1");
            var mockSensitiveDataProvider = new Mock<ISensitiveDataProvider>();
            mockSensitiveDataProvider.Setup(x => x.GetValuesAsync(It.IsAny<List<string>>(), It.IsAny<bool>())).ReturnsAsync(sensitiveData);

            var secureConfigurationStore = new SecureConfigurationStore(mockConsulStore.Object, mockSensitiveDataProvider.Object);

            var result = await secureConfigurationStore.GetAllAsync();

            Assert.Equal(consulConfiguration.Count, result.Count);
            mockConsulStore.Verify(x => x.GetAllAsync(), Times.Once);
            mockSensitiveDataProvider.Verify(x => x.GetValuesAsync(It.IsAny<List<string>>(), It.IsAny<bool>()), Times.Once);
        }
    }
}
