using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Moq;
using Tavisca.Common.Plugins.Configuration;
using JsonSerializer = Tavisca.Common.Plugins.Configuration.JsonSerializer;
using System.Collections.Specialized;
using Tavisca.Platform.Common.Configurations;

namespace Tavisca.Platform.Common.Tests
{
    [TestClass]
    public class ConfigurationProviderTest
    {

        [TestMethod]
        public async Task GetGlobalConfiguration_ShouldThrowConfigurationException_WhenSectionNameIsNull()
        {
            var applicationName = "applicationName";
            var reader = new ConfigurationProvider(applicationName);
            var configurationException = await Xunit.Assert.ThrowsAsync<ConfigurationException>(() => reader.GetGlobalConfigurationAsStringAsync(null, "redisCache"));
            Assert.IsNotNull(configurationException);
            Assert.IsInstanceOfType(configurationException, typeof(ConfigurationException));
        }

        [TestMethod]
        public async Task GetGlobalConfiguration_ShouldThrowConfigurationException_WhenSectionNameIsEmpty()
        {
            var applicationName = "applicationName";
            var reader = new ConfigurationProvider(applicationName);
            var configurationException = await Xunit.Assert.ThrowsAsync<ConfigurationException>(() => reader.GetGlobalConfigurationAsStringAsync(string.Empty, "redisCache"));
            Assert.IsNotNull(configurationException);
            Assert.IsInstanceOfType(configurationException, typeof(ConfigurationException));
        }

        [TestMethod]
        public async Task GetGlobalConfiguration_ShouldThrowConfigurationException_WhenKeyIsNull()
        {
            var applicationName = "applicationName";
            var reader = new ConfigurationProvider(applicationName);
            var configurationException = await Xunit.Assert.ThrowsAsync<ConfigurationException>(() => reader.GetGlobalConfigurationAsStringAsync("connectionStrings", null));
            Assert.IsNotNull(configurationException);
            Assert.IsInstanceOfType(configurationException, typeof(ConfigurationException));
        }

        [TestMethod]
        public async Task GetGlobalConfiguration_ShouldThrowConfigurationException_WhenKeyIsEmpty()
        {
            var applicationName = "applicationName";
            var reader = new ConfigurationProvider(applicationName);
            var configurationException = await Xunit.Assert.ThrowsAsync<ConfigurationException>(() => reader.GetGlobalConfigurationAsStringAsync("connectionStrings", ""));
            Assert.IsNotNull(configurationException);
            Assert.IsInstanceOfType(configurationException, typeof(ConfigurationException));
        }

        [TestMethod]
        public async Task GetGlobalConfiguration_ShouldReturnNull_WhenSectionNameKeyIsInvalid()
        {
            var applicationName = "applicationName";
            var configStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();
            var configReader = new ConfigurationProvider(configStoreMock.Object, new JsonSerializer(), applicationName, sensitiveDataProviderMock.Object);
            configStoreMock.Setup(store => store.GetAsync("global", applicationName, "invalildSection", "invalidKey")).ReturnsAsync(null);
            var configuration = await configReader.GetGlobalConfigurationAsStringAsync("invalildSection", "invalidKey");
            Assert.IsNull(configuration);
        }

        [TestMethod]
        public async Task GetGlobalConfiguration_ShouldReturnIConfigurationValue_WhenSectionNameKeyIsValid()
        {
            var applicationName = "applicationName";
            var configStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();
            var configReader = new ConfigurationProvider(configStoreMock.Object, new JsonSerializer(), applicationName, sensitiveDataProviderMock.Object);
            configStoreMock.Setup(store => store.GetAsync("global", applicationName, "validSection", "validKey")).ReturnsAsync("validValue");
            var configuration = await configReader.GetGlobalConfigurationAsStringAsync("validSection", "validKey");
            Assert.IsNotNull(configuration);
            Assert.AreEqual("validValue", configuration);
        }




        [TestMethod]
        public async Task GetTenantConfiguration_ShouldThrowConfigurationException_WhenTenantIdIsNull()
        {
            var applicationName = "applicationName";
            var reader = new ConfigurationProvider(applicationName);
            var configurationException = await Xunit.Assert.ThrowsAsync<ConfigurationException>(() => reader.GetTenantConfigurationAsStringAsync(null, "sectionName", "key"));
            Assert.IsNotNull(configurationException);
            Assert.IsInstanceOfType(configurationException, typeof(ConfigurationException));
        }

        [TestMethod]
        public async Task GetTenantConfiguration_ShouldThrowConfigurationException_WhenTenantIdIsEmpty()
        {
            var applicationName = "applicationName";
            var reader = new ConfigurationProvider(applicationName);
            var configurationException = await Xunit.Assert.ThrowsAsync<ConfigurationException>(() => reader.GetTenantConfigurationAsStringAsync("", "sectionName", "key"));
            Assert.IsNotNull(configurationException);
            Assert.IsInstanceOfType(configurationException, typeof(ConfigurationException));
        }

        [TestMethod]
        public async Task GetTenantConfiguration_ShouldThrowConfigurationException_WhenSectionNameIsNull()
        {
            var applicationName = "applicationName";
            var reader = new ConfigurationProvider(applicationName);
            var configurationException = await Xunit.Assert.ThrowsAsync<ConfigurationException>(() => reader.GetTenantConfigurationAsStringAsync("bac", null, "key"));
            Assert.IsNotNull(configurationException);
            Assert.IsInstanceOfType(configurationException, typeof(ConfigurationException));
        }


        [TestMethod]
        public async Task GetTenantConfiguration_ShouldThrowConfigurationException_WhenSectionNameIsEmpty()
        {
            var applicationName = "applicationName";
            var reader = new ConfigurationProvider(applicationName);
            var configurationException = await Xunit.Assert.ThrowsAsync<ConfigurationException>(() => reader.GetTenantConfigurationAsStringAsync("bac", "", "key"));
            Assert.IsNotNull(configurationException);
            Assert.IsInstanceOfType(configurationException, typeof(ConfigurationException));
        }

        [TestMethod]
        public async Task GetTenantConfiguration_ShouldThrowConfigurationException_WhenKeyIsNull()
        {
            var applicationName = "applicationName";
            var reader = new ConfigurationProvider(applicationName);
            var configurationException = await Xunit.Assert.ThrowsAsync<ConfigurationException>(() => reader.GetTenantConfigurationAsStringAsync("bac", "section", null));
            Assert.IsNotNull(configurationException);
            Assert.IsInstanceOfType(configurationException, typeof(ConfigurationException));
        }

        [TestMethod]
        public async Task GetTenantConfiguration_ShouldThrowConfigurationException_WhenKeyIsEmpty()
        {
            var applicationName = "applicationName";
            var reader = new ConfigurationProvider(applicationName);
            var configurationException = await Xunit.Assert.ThrowsAsync<ConfigurationException>(() => reader.GetTenantConfigurationAsStringAsync("bac", "section", ""));
            Assert.IsNotNull(configurationException);
            Assert.IsInstanceOfType(configurationException, typeof(ConfigurationException));
        }


        [TestMethod]
        public async Task GetTenantConfiguration_ShouldReturnNull_WhenTenantIdSectionNameKeyIsInvalid()
        {
            var applicationName = "applicationName";
            var configStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();
            var configReader = new ConfigurationProvider(configStoreMock.Object, new JsonSerializer(), applicationName, sensitiveDataProviderMock.Object);
            configStoreMock.Setup(store => store.GetAsync("invalidTenantId", applicationName, "invalildSection", "invalidKey")).ReturnsAsync(null);
            var configuration = await configReader.GetTenantConfigurationAsStringAsync("invalidTenantId", "invalildSection", "invalidKey");
            Assert.IsNull(configuration);
        }

        [TestMethod]
        public async Task GetTenantConfiguration_ShouldReturnIConfigurationValue_WhenTenantIdSectionNameKeyIsValid()
        {
            var applicationName = "applicationName";
            var configStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();
            var configReader = new ConfigurationProvider(configStoreMock.Object, new JsonSerializer(), applicationName, sensitiveDataProviderMock.Object);
            configStoreMock.Setup(store => store.GetAsync("validTenantId", applicationName, "validSection", "validKey")).ReturnsAsync("validValue");
            var configuration = await configReader.GetTenantConfigurationAsStringAsync("validTenantId", "validSection", "validKey");
            Assert.IsNotNull(configuration);
            Assert.AreEqual("validValue", configuration);
        }

        [TestMethod]
        public async Task GetTenantConfiguration_ShouldReturnIConfigurationValue_WhenTenantIdSectionNameKeyIsValidAndWithCustomJsonSerializer()
        {
            var applicationName = "applicationName";
            var configStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();
            var configReader = new ConfigurationProvider(configStoreMock.Object, new JsonSerializer(), applicationName, sensitiveDataProviderMock.Object);
            configStoreMock.Setup(store => store.GetAsync("validTenantId", applicationName, "validSection", "validKey")).ReturnsAsync("validValue");
            var configuration = await configReader.GetTenantConfigurationAsStringAsync("validTenantId", "validSection", "validKey");
            Assert.IsNotNull(configuration);
            Assert.AreEqual("validValue", configuration);
        }


        [TestMethod]
        public void Constructor_ShouldThrowConfigurationException_WhenStoreAndSerializerAreNull()
        {
            var applicationName = "applicationName";
            var configurationException = Xunit.Assert.Throws<ConfigurationException>(() => new ConfigurationProvider(null, null, applicationName,null));
            Assert.IsNotNull(configurationException);
        }

        [TestMethod]
        public void Constructor_ShouldCreateAReaderInstance_WhenStoreAndSerializerAreValid()
        {
            var applicationName = "applicationName";
            var configStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();
            var ConfigurationProvider = new ConfigurationProvider(configStoreMock.Object, new JsonSerializer(), applicationName, sensitiveDataProviderMock.Object);
            Assert.IsNotNull(ConfigurationProvider);
        }

        //[TestMethod]
        //public void Constructor_ShouldCreateAReaderInstance_WhenStoreAndSerializerAreValidAndConsulClientParameterized()
        //{
        //    var ConfigurationProvider = new ConfigurationProvider(new ConsulConfigurationStore(new ConsulClient("localhost:8500")), new JsonSerializer());
        //    Assert.IsNotNull(ConfigurationProvider);
        //}

        [TestMethod]
        public async Task GetGlobalConfiguration_ShouldReturnIConfigurationValue_WhenSectionNameKeyIsValid_WithStore()
        {
            var applicationName = "appplicationName";
            var configStoreMock = new Mock<IConfigurationStore>();
            ConfigurationProvider.Configuration.WithRemoteStore(configStoreMock.Object).Apply();
            var configReader = new ConfigurationProvider(applicationName);
            configStoreMock.Setup(store => store.GetAsync("global", applicationName, "validSection", "validKey")).ReturnsAsync("validValue");
            var configuration = await configReader.GetGlobalConfigurationAsStringAsync("validSection", "validKey");
            Assert.IsNotNull(configuration);
            Assert.AreEqual("validValue", configuration);
        }

        [TestMethod]
        public async Task GetGlobalConfiguration_ShouldReturnIConfigurationValue_WhenSectionNameKeyIsValid_WithSerializer()
        {
            var applicationName = "appplicationName";
            var configStoreMock = new Mock<IConfigurationStore>();
            ConfigurationProvider.Configuration.WithRemoteStore(configStoreMock.Object).Apply();
            configStoreMock.Setup(store => store.GetAsync("global", applicationName, "validSection", "validKey")).ReturnsAsync("validValue");

            var jsonSerializerMock = new Mock<IJsonSerializer>();
            ConfigurationProvider.Configuration.WithSerializer(jsonSerializerMock.Object).Apply();
            ConfigurationProvider.Configuration.WithRemoteStore(configStoreMock.Object);
            var configReader = new ConfigurationProvider(applicationName);

            jsonSerializerMock.Setup(store => store.Deserialize<string>("validKey")).Returns("validValue");
            var configuration = await configReader.GetGlobalConfigurationAsStringAsync("validSection", "validKey");
            Assert.IsNotNull(configuration);
            Assert.AreEqual("validValue", configuration);
        }



        #region featureflag test cases

        [TestMethod]
        public async Task GetFeatureFlagSettingsAsStringAsync_ShouldReturnIConfigurationValue_fromTenantId_WhenKeyIsValid()
        {
            var applicationName = "applicationName";
            var configStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();
            var configReader = new ConfigurationProvider(configStoreMock.Object, new JsonSerializer(), applicationName, sensitiveDataProviderMock.Object);
            configStoreMock.Setup(store => store.GetAsync("demoTenant", applicationName, "feature_flag_settings", "validKey")).ReturnsAsync("validValue");
            var configuration = await configReader.GetFeatureFlagSettingsAsStringAsync("demoTenant", "validKey");
            Assert.IsNotNull(configuration);
            Assert.AreEqual("validValue", configuration);
        }
        [TestMethod]
        public async Task GetFeatureFlagSettingsAsStringAsync_ShouldThrowException_fromTenantId_WhenKeyIsNull()
        {
            var applicationName = "applicationName";
            var configReader = new ConfigurationProvider(applicationName);
            var configurationException = await Xunit.Assert.ThrowsAsync<ConfigurationException>(() => configReader.GetFeatureFlagSettingsAsStringAsync("demoTenant", null));
            Assert.IsNotNull(configurationException);
            Assert.IsInstanceOfType(configurationException, typeof(ConfigurationException));
        }

        [TestMethod]
        public async Task GetFeatureFlagSettingsAsStringAsync_ShouldReturnNullValue_fromTenantId_WhenKeyIsInvalid()
        {
            var applicationName = "applicationName";
            var configStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();
            var configReader = new ConfigurationProvider(configStoreMock.Object, new JsonSerializer(), applicationName, sensitiveDataProviderMock.Object);
            configStoreMock.Setup(store => store.GetAsync("demoTenant", applicationName, "feature_flag_settings", "validKey")).ReturnsAsync(null);
            var configuration = await configReader.GetFeatureFlagSettingsAsStringAsync("demoTenant", "invalidKey");
            Assert.IsNull(configuration);
        }

        [TestMethod]
        public async Task GetFeatureFlagSettingsAsStringAsync_ShouldReturnNullValue_fromDefault_WhenKeyIsValid()
        {
            var applicationName = "applicationName";
            var configStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();
            var configReader = new ConfigurationProvider(configStoreMock.Object, new JsonSerializer(), applicationName, sensitiveDataProviderMock.Object);
            configStoreMock.Setup(store => store.GetAsync("default", applicationName, "feature_flag_settings", "validKey")).ReturnsAsync("validValue");
            var configuration = await configReader.GetFeatureFlagSettingsAsStringAsync("demoTenant", "validKey");
            Assert.IsNotNull(configuration);
            Assert.AreEqual("validValue", configuration);
        }


        [TestMethod]
        public async Task GetFeatureFlagSettingsAsStringAsync_ShouldReturnNullValue_fromGlobal_WhenKeyIsValid()
        {
            var applicationName = "applicationName";
            var configStoreMock = new Mock<IConfigurationStore>();
            var sensitiveDataProviderMock = new Mock<ISensitiveDataProvider>();
            var configReader = new ConfigurationProvider(configStoreMock.Object, new JsonSerializer(), applicationName, sensitiveDataProviderMock.Object);
            configStoreMock.Setup(store => store.GetAsync("global", applicationName, "feature_flag_settings", "validKey")).ReturnsAsync("validValue");
            var configuration = await configReader.GetFeatureFlagSettingsAsStringAsync(null, "validKey");
            Assert.IsNotNull(configuration);
            Assert.AreEqual("validValue", configuration);
        }

        #endregion
    }
}
