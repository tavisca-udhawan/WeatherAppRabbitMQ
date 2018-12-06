using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Moq;
using System.Threading;
using System.Collections.Generic;
using Tavisca.Common.Plugins.Configuration;
using Tavisca.Platform.Common.Monotone;
using Tavisca.Platform.Common.LockManagement;
using Tavisca.Common.Plugins.Aerospike;
using System.Collections.Specialized;

namespace Tavisca.Platform.Common.Tests
{
    [Ignore]
    [TestClass]
    public class AerospikeInstanceDataStoreTest
    {

        [TestMethod]
        public async Task Aerospike_GetInstanceList_Valid()
        {
            var instanceDS = new AerospikeInstanceDataStore(new ConfigurationProvider("montone_id_generator"));

            var instanceList = instanceDS.GetInstanceList();

            Assert.IsNotNull(instanceList);

        }

        [TestMethod]
        public async Task Aerospike_AddOrUpdate_InstanceInfo_Valid()
        {
            var instanceDS = new AerospikeInstanceDataStore(new ConfigurationProvider("montone_id_generator"));

            var timestamp = DateTime.UtcNow;
            var instance = new InstanceInfo(10, 11, 23, timestamp);

            instanceDS.AddOrUpdate(instance);

            var instanceList = instanceDS.GetInstanceList();

            Assert.IsNotNull(instanceList);

            Assert.IsTrue(instanceList.Keys.Count > 0);

            Assert.AreEqual(10, instanceList[instance.GetUniqueId()].RegionId);
            Assert.AreEqual(11, instanceList[instance.GetUniqueId()].ZoneId);
            Assert.AreEqual(12, instanceList[instance.GetUniqueId()].InstanceId);

        }

        [TestMethod]
        public async Task Aerospike_AddOrUpdate_TimeStamp_Valid()
        {
            var instanceDS = new AerospikeInstanceDataStore(new ConfigurationProvider("montone_id_generator"));

            var timestamp = DateTime.UtcNow;
            var instance = new InstanceInfo(10, 11, 23, timestamp);

            instanceDS.AddOrUpdate(instance);

            var instanceList = instanceDS.GetInstanceList();

            Assert.IsNotNull(instanceList);

            Assert.IsTrue(instanceList.Keys.Count > 0);
                        
            Assert.AreEqual(timestamp.Hour, instanceList[instance.GetUniqueId()].UpdateTimestamp.Hour);
            Assert.AreEqual(timestamp.Minute, instanceList[instance.GetUniqueId()].UpdateTimestamp.Minute);
            Assert.AreEqual(timestamp.Second, instanceList[instance.GetUniqueId()].UpdateTimestamp.Second);

        }

    }


}
