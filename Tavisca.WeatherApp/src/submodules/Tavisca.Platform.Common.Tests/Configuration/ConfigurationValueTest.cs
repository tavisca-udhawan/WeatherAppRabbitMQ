using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using Newtonsoft.Json;
using Moq;
using Tavisca.Common.Plugins.Configuration;
using JsonSerializer = Tavisca.Common.Plugins.Configuration.JsonSerializer;

namespace Tavisca.Platform.Common.Tests
{
    [TestClass]
    public class ConfigurationValueTest
    {
        [TestMethod]
        public void GetAsString_ShouldReturnNull_WhenValueIsNullAndNoDefaultValue()
        {
            var configValue = new ConfigurationValue(null, new JsonSerializer());
            var configAsString =  configValue.GetAsString();
            Assert.IsNull(configAsString);
        }

        [TestMethod]
        public void GetAsString_ShouldReturnDefaultValue_WhenValueIsNull()
        {
            var configValue = new ConfigurationValue(null, new JsonSerializer());
            var configAsString = configValue.GetAsString("defaultValue");
            Assert.IsNotNull(configAsString);
            Assert.AreEqual(configAsString,"defaultValue");

            var configAsStringWithNullDefaultValue = configValue.GetAsString(null);
            Assert.IsNull(configAsStringWithNullDefaultValue);
        }

        [TestMethod]
        public void GetAsString_ShouldReturnValue_WhenValueIsValid()
        {
            var configValue = new ConfigurationValue("validValue", new JsonSerializer());
            var configAsString = configValue.GetAsString("defaultValue");
            Assert.IsNotNull(configAsString);
            Assert.AreEqual(configAsString, "validValue");
        }

        [TestMethod]
        public void GetAsString_ShouldReturnNull_WhenValueIsEmptyAndNoDefaultValue()
        {
            var configValue = new ConfigurationValue("", new JsonSerializer());
            var configAsString = configValue.GetAsString();
            Assert.IsNull(configAsString);
        }

        [TestMethod]
        public void GetAsString_ShouldReturnDefaultValue_WhenValueIsEmpty()
        {
            var configValue = new ConfigurationValue("", new JsonSerializer());
            var configAsString = configValue.GetAsString("defaultValue");
            Assert.IsNotNull(configAsString);
            Assert.AreEqual(configAsString, "defaultValue");

            var configAsStringWithNullDefaultValue = configValue.GetAsString(null);
            Assert.IsNull(configAsStringWithNullDefaultValue);

        }

        [TestMethod]
        public void GetAsNameValueCollection_ShouldReturnNull_WhenValueIsNullAndNoDefaultValue()
        {
            var configValue = new ConfigurationValue(null, new JsonSerializer());
            var configAsNameValueCollection = configValue.GetAsNameValueCollcetion();
            Assert.IsNull(configAsNameValueCollection);
        }

        [TestMethod]
        public void GetAsNameValueCollection_ShouldReturnDefaultValue_WhenValueIsNull()
        {
            var defaultValue = new NameValueCollection();
            defaultValue.Add("defaultKey", "defaultValue");
            var configValue = new ConfigurationValue(null, new JsonSerializer());
            var configAsNameValueCollection = configValue.GetAsNameValueCollcetion(defaultValue);
            Assert.IsNotNull(configAsNameValueCollection);
            Assert.AreEqual(defaultValue["defaultKey"],configAsNameValueCollection["defaultKey"]);
        }

        [TestMethod]
        public void GetAsNameValueCollection_ShouldReturnValue_WhenValueIsValid_Scenario_1()
        {
            var value = "{\"firstKey\":\"firstValue\",\"secondKey\":\"secondValue\",\"thirdKey\":\"thirdValue\"}";
            var configValue = new ConfigurationValue(value, new JsonSerializer());
            var configAsNameValueCollection = configValue.GetAsNameValueCollcetion();
            Assert.IsNotNull(configAsNameValueCollection);
            Assert.AreEqual(configAsNameValueCollection["firstKey"],"firstValue");
            Assert.AreEqual(configAsNameValueCollection["secondKey"], "secondValue");
        }

        [TestMethod]
        public void GetAsNameValueCollection_ShouldReturnValue_WhenValueIsValid_Scenario_2()
        {
            var value = "{\"firstKey\":null,\"secondKey\":null,\"thirdKey\":\"thirdValue\"}";
            var configValue = new ConfigurationValue(value, new JsonSerializer());
            var configAsNameValueCollection = configValue.GetAsNameValueCollcetion();
            Assert.IsNotNull(configAsNameValueCollection);
            Assert.AreEqual(configAsNameValueCollection["firstKey"], null);
            Assert.AreEqual(configAsNameValueCollection["secondKey"], null);
            Assert.AreEqual(configAsNameValueCollection["thirdKey"], "thirdValue");
        }



        [TestMethod]
        public void GetAsNameValueCollection_ShouldThrowSerializationException_WhenValueIsInvalid()
        {
            var value = "{\"redisConnection\":{\"ipAddress\":\"192.168.2.2\",\"port\":\"8600\"}}";
            var configValue = new ConfigurationValue(value, new JsonSerializer());
            var exception = Xunit.Assert.Throws<SerializationException>(() => configValue.GetAsNameValueCollcetion());
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void GetAsNameValueCollection_ShouldReturnNull_WhenValueIsNotJsonReadable()
        {
            var value = "{}";
            var configValue = new ConfigurationValue(value, new JsonSerializer());
            var nameValueCollection = configValue.GetAsNameValueCollcetion();
            Assert.IsNull(nameValueCollection);
        }

        [TestMethod]
        public void GetAsT_ShouldReturnNull_WhenValueIsNullAndNoDefaultValue()
        {
            var configValue = new ConfigurationValue(null, new JsonSerializer());
            var configAsT = configValue.GetAs<Configurations>();
            Assert.IsNull(configAsT);
        }

        [TestMethod]
        public void GetAsT_ShouldReturnDefaultValue_WhenValueIsNull()
        {
            var defaultValue = new Configurations();
            defaultValue.RedisConnection = new Connections("192.168.2.2","8800");
            var configValue = new ConfigurationValue(null, new JsonSerializer());
            var configAsT = configValue.GetAs(defaultValue);
            Assert.IsNotNull(configAsT);
            Assert.AreEqual(configAsT.RedisConnection.IpAddress, defaultValue.RedisConnection.IpAddress);
            Assert.AreEqual(configAsT.RedisConnection.Port, defaultValue.RedisConnection.Port);
        }

        [TestMethod]
        public void GetAsT_ShouldReturnValue_WhenValueIsValid()
        {
            var value = "{\"redisConnection\":{\"ipAddress\":\"192.168.2.2\",\"port\":\"8600\"}}";
            var configValue = new ConfigurationValue(value, new JsonSerializer());
            var configAsT = configValue.GetAs<Configurations>();

            Assert.IsNotNull(configAsT);
            Assert.AreEqual(configAsT.RedisConnection.IpAddress,"192.168.2.2");
        }

        [TestMethod]
        public void GetAsT_ShouldReturnStringValue_WhenValueIsString()
        {
            var value = "stringValue";
            var configValue = new ConfigurationValue(value, new JsonSerializer());
            var configAsT = configValue.GetAs<string>();
        }

        [TestMethod]
        public void GetAsT_ShouldThrowSerializationException_WhenValueIsNotSerializable()
        {
            var value = "{\"redisConnection\": \"192.168.2.2\"}";
            var configValue = new ConfigurationValue(value, new JsonSerializer());
            var exception = Xunit.Assert.Throws<SerializationException>(() => configValue.GetAs<Configurations>());
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void GetAsT_ShouldThrowSerializationException_WhenValueIsNotJsonReadable()
        {
            var value = "something";
            var configValue = new ConfigurationValue(value, new JsonSerializer());
            var exception = Xunit.Assert.Throws<SerializationException>(() => configValue.GetAs<Configurations>());
            Assert.IsNotNull(exception);
        }

        [TestMethod]
        public void GetAsT_ShouldReturnEmptyObject_WhenValueIsInValid()
        {
            var value = "{\"firstKey\":\"firstValue\",\"secondKey\":\"secondValue\",\"thirdKey\":\"thirdValue\"}";
            var configValue = new ConfigurationValue(value, new JsonSerializer());
            var configAsT = configValue.GetAs<Configurations>();

            Assert.IsNotNull(configAsT);
            Assert.IsNull(configAsT.RedisConnection);
            Assert.IsNull(configAsT.ConsulConnection);
        }


        private class Configurations
        {
            public Connections RedisConnection { get; set; }

            public Connections ConsulConnection { get; set; }
        }

        private class Connections
        {
            public Connections(string ipAddress,string port)
            {
                IpAddress = ipAddress;
                Port = port;
            }

            public string IpAddress { get; set; }
            public string Port { get; set; }
        }
    }
}
