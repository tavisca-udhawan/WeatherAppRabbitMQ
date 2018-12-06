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
using Tavisca.Platform.Common.Configurations;
using System.Collections.Specialized;
using System.Linq;

namespace Tavisca.Platform.Common.Tests
{

    [TestClass]
    public class WebFarmConfigurationTest
    {

        NameValueCollection _settings = new NameValueCollection();

        [TestMethod]
        public async Task GetInstanceId_FirstInstance_Valid()
        {
            var instanceDataStoreMock = new Mock<IInstanceDataStore>();
            var lockProviderMock = new Mock<ILockProvider>();
            var configProviderMock = new Mock<IConfigurationProvider>();
            configProviderMock.Setup(cp => cp.GetGlobalConfiguration<int>("", "","")).Returns(1800);

            lockProviderMock.Setup(loc => loc.TryGetLockAsync("abc", LockType.Write, CancellationToken.None)).ReturnsAsync(true);
            instanceDataStoreMock.Setup(ds => ds.GetInstanceList()).Returns(new Dictionary<string, InstanceInfo>());
            instanceDataStoreMock.Setup(ds => ds.AddOrUpdate(new InstanceInfo(0, 0, 0, DateTime.UtcNow)));

            var webFarm = new WebFarmConfiguration(instanceDataStoreMock.Object, lockProviderMock.Object, configProviderMock.Object);
            webFarm.Intialize();
            var instanceId = webFarm.GetInstanceId();
            Assert.IsNotNull(instanceId);
            Assert.AreEqual((UInt32)(0), instanceId);
        }

        [TestMethod]
        public async Task GetSameInstanceId_MultipleRetrieval_Valid()
        {
            var instanceDataStoreMock = new Mock<IInstanceDataStore>();
            var lockProviderMock = new Mock<ILockProvider>();
            var configProviderMock = new Mock<IConfigurationProvider>();
            configProviderMock.Setup(cp => cp.GetGlobalConfiguration<int>("", "","")).Returns(1800);

            lockProviderMock.Setup(loc => loc.TryGetLockAsync("abc", LockType.Write, CancellationToken.None)).ReturnsAsync(true);
            instanceDataStoreMock.Setup(ds => ds.GetInstanceList()).Returns(new Dictionary<string, InstanceInfo>());
            instanceDataStoreMock.Setup(ds => ds.AddOrUpdate(new InstanceInfo(0, 0, 0, DateTime.UtcNow)));


            var webFarm = new WebFarmConfiguration(instanceDataStoreMock.Object, lockProviderMock.Object, configProviderMock.Object);
            webFarm.Intialize();
            var instanceId1 = webFarm.GetInstanceId();

            var instanceId2 = webFarm.GetInstanceId();

            Assert.AreEqual(instanceId1, instanceId2);
        }

        [TestMethod]
        public async Task GetInstanceId_OutOfUse_Valid()
        {
            var instanceDataStoreMock = new Mock<IInstanceDataStore>();
            var lockProviderMock = new Mock<ILockProvider>();
            var configProviderMock = new Mock<IConfigurationProvider>();
            configProviderMock.Setup(cp => cp.GetGlobalConfiguration<int>("", "")).Returns(1800);
            lockProviderMock.Setup(loc => loc.TryGetLockAsync("abc", LockType.Write, CancellationToken.None)).ReturnsAsync(true);

            var instanceList = new Dictionary<string, InstanceInfo>();
            var instance = new InstanceInfo(0, 0, 1, DateTime.UtcNow.Subtract(new TimeSpan(2, 0, 0)));
            instanceList.Add(instance.GetUniqueId(),instance);
            instanceDataStoreMock.Setup(ds => ds.GetInstanceList()).Returns(instanceList);

            var webFarm = new WebFarmConfiguration(instanceDataStoreMock.Object, lockProviderMock.Object, configProviderMock.Object);

            webFarm.Intialize();
            var instanceId = webFarm.GetInstanceId();

            Assert.AreEqual(instanceId, (UInt32)1);
        }

        [TestMethod]
        public async Task GetInstanceId_IncrementedId_Valid()
        {
            var instanceDataStoreMock = new Mock<IInstanceDataStore>();
            var lockProviderMock = new Mock<ILockProvider>();
            lockProviderMock.Setup(loc => loc.TryGetLockAsync("abc", LockType.Write, CancellationToken.None)).ReturnsAsync(true);
            var configProviderMock = new Mock<IConfigurationProvider>();
            configProviderMock.Setup(cp => cp.GetGlobalConfiguration<int>("", "")).Returns(1800);
            var instanceList = new Dictionary<string, InstanceInfo>();

            var instance1 = new InstanceInfo(0, 0, 0, DateTime.UtcNow.Subtract(new TimeSpan(0, 30, 0)));

            var instance2 = new InstanceInfo(0, 0, 1, DateTime.UtcNow.Subtract(new TimeSpan(0, 15, 0)));

            instanceList.Add(instance1.GetUniqueId(),instance1);
            instanceList.Add(instance2.GetUniqueId(),instance2);

            instanceDataStoreMock.Setup(ds => ds.GetInstanceList()).Returns(instanceList);

            var webFarm = new WebFarmConfiguration(instanceDataStoreMock.Object, lockProviderMock.Object, configProviderMock.Object);

            webFarm.Intialize();
            var instanceId = webFarm.GetInstanceId();

            Assert.AreEqual(instanceId, (UInt32)2);
        }

        [TestMethod]
        public async Task CheckHeartBeat_TimerSetting_Valid()
        {
            var instanceDataStore = new DummyInstanceStore();
            var lockProviderMock = new Mock<ILockProvider>();
            lockProviderMock.Setup(loc => loc.TryGetLockAsync("abc", LockType.Write, CancellationToken.None)).ReturnsAsync(true);
            var configProviderMock = new Mock<IConfigurationProvider>();
            _settings.Add("reset_time_sec", "5");
            configProviderMock.Setup(cp => cp.GetGlobalConfigurationAsNameValueCollection("montone_id_generator", "webfarm_settings", "heartbeat_settings")).Returns(_settings);

            var webFarm = new WebFarmConfiguration(instanceDataStore, lockProviderMock.Object, configProviderMock.Object);
            webFarm.Intialize();

            var instanceInfo = new InstanceInfo(webFarm.GetRegionId(), webFarm.GetZoneId(), webFarm.GetInstanceId(),DateTime.UtcNow);

            var datetime1 = instanceDataStore.InstanceList[instanceInfo.GetUniqueId()].UpdateTimestamp;

            Thread.Sleep(6000);
            
            var datetime2 = instanceDataStore.InstanceList[instanceInfo.GetUniqueId()].UpdateTimestamp;
            Assert.IsTrue(datetime2 > datetime1);

        }

        [TestMethod]
        public async Task MultiRegion_AddSameInstanceId_Valid()
        {
            var instanceDataStoreMock = new Mock<IInstanceDataStore>();
            var lockProviderMock = new Mock<ILockProvider>();
            lockProviderMock.Setup(loc => loc.TryGetLockAsync("abc", LockType.Write, CancellationToken.None)).ReturnsAsync(true);
            var configProviderMock = new Mock<IConfigurationProvider>();
            configProviderMock.Setup(cp => cp.GetGlobalConfiguration<int>("", "","")).Returns(1800);
            
            var instanceList = new Dictionary<string, InstanceInfo>();

            var instance1 = new InstanceInfo(0, 0, 0, DateTime.UtcNow);
            var instance2 = new InstanceInfo(1, 0, 0, DateTime.UtcNow);

            instanceList.Add(instance1.GetUniqueId(), instance1);
            instanceList.Add(instance2.GetUniqueId(), instance2);

            configProviderMock.Setup(cp => cp.GetGlobalConfiguration<uint>("montone_id_generator", "webfarm_settings", "webfarm_regionid")).Returns(2);

            instanceDataStoreMock.Setup(ds => ds.GetInstanceList()).Returns(instanceList);

            var webFarm = new WebFarmConfiguration(instanceDataStoreMock.Object, lockProviderMock.Object, configProviderMock.Object);

            webFarm.Intialize();
            var instanceId = webFarm.GetInstanceId();

            Assert.AreEqual(instanceId, (UInt32)0);
        }



    }

    public class DummyInstanceStore : IInstanceDataStore
    {
        public readonly Dictionary<string, InstanceInfo> InstanceList;

        public DummyInstanceStore()
        {
            InstanceList = new Dictionary<string, InstanceInfo>();
        }

        public void AddOrUpdate(InstanceInfo instanceinfo)
        {
            InstanceList[instanceinfo.GetUniqueId()] = instanceinfo;
        }

        public Dictionary<string, InstanceInfo> GetInstanceList()
        {
            return InstanceList;
        }
    }


}
