using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.LockManagement;

namespace Tavisca.Platform.Common.Monotone
{
    public class WebFarmConfiguration : IIdConfiguration
    {
        private readonly IInstanceDataStore _instanceConfigurationStore;

        private readonly IConfigurationProvider _configurationProvider;

        private GlobalLock _globalLock;

        private static System.Threading.Timer _heartbeatTimer;

        private static uint _instanceId;

        private static uint _regionId;

        private static uint _zoneId;

        private NameValueCollection _heartBeatSettings;

        private readonly string _monotoneIdGenerator = "montone_id_generator";

        private readonly string _heartbeatSettings = "heartbeat_settings";

        private readonly string _heartbeatResetTime = "reset_time_sec";

        private readonly string _heartbeatElapsedTime = "elapsed_time_sec";

        private readonly string _webfarmRegionId = "webfarm_regionid";

        private readonly string _lockKey = "::instance_id_lock::";

        public const string _webfarmSettings = "webfarm_settings";

        public WebFarmConfiguration(IInstanceDataStore instanceConfigurationStore, ILockProvider lockProvider, IConfigurationProvider configurationProvider)
        {
            _instanceConfigurationStore = instanceConfigurationStore;
            _heartBeatSettings = configurationProvider.GetGlobalConfigurationAsNameValueCollection(_monotoneIdGenerator, _webfarmSettings, _heartbeatSettings)
                        ?? new NameValueCollection();
            _globalLock = new GlobalLock(lockProvider);
            _configurationProvider = configurationProvider;
        }

        public uint GetInstanceId()
        {
            return _instanceId;
        }

        public uint GetRegionId()
        {
            return _regionId;
        }

        public uint GetZoneId()
        {
            return _zoneId;
        }

        public void Intialize()
        {

            var padlock = GetLock();
            using (padlock)
            {
                SetRegionId();

                SetZoneId();

                SetInstanceId();
               
            }
        }

        private void SetZoneId()
        {
            _zoneId = 0;
        }

        private void SetRegionId()
        {
           _regionId = _configurationProvider.GetGlobalConfiguration<uint>(_monotoneIdGenerator, _webfarmSettings, _webfarmRegionId);
        }

        private int GetHeartbeatResetDuration()
        {
            int duration;
            return int.TryParse(_heartBeatSettings[_heartbeatResetTime], out duration)
                            ? duration
                            : 1800;
        }

        private int GetHeartbeatElaspedDuration()
        {
            int duration;
            return int.TryParse(_heartBeatSettings[_heartbeatElapsedTime], out duration)
                            ? duration
                            : 3600;
        }

        private IDisposable GetLock()
        {
            var waitHandle = new ManualResetEvent(false);

            IDisposable padLock = null;

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    padLock = await _globalLock.EnterWriteLock(_lockKey);
                }
                finally
                {
                    waitHandle.Set();
                }
            });
            waitHandle.WaitOne();

            return padLock;

        }



        private void SetInstanceId()
        {
            var instances = _instanceConfigurationStore.GetInstanceList();

            _instanceId = GetFreeInstanceId(instances);

            UpdateTimeStamp();

            SetInstaceAsAlive();

        }

        private void SetInstaceAsAlive()
        {
            var duration = GetHeartbeatResetDuration();
            _heartbeatTimer = new Timer(o => UpdateTimeStamp(), null, new TimeSpan(0, 0, duration), new TimeSpan(0, 0, duration));
        }

        private void UpdateTimeStamp()
        {
            _instanceConfigurationStore.AddOrUpdate(new InstanceInfo() { InstanceId = _instanceId, RegionId = _regionId, ZoneId = _zoneId ,UpdateTimestamp = DateTime.UtcNow });
        }

        private uint GetFreeInstanceId(Dictionary<string, InstanceInfo> instancesInfo)
        {
            uint instanceId = 0;
            bool isInstanceFree = false;
            var elaspedDuration = GetHeartbeatElaspedDuration();

            if (instancesInfo == null)
                return instanceId;

            var instancesForCurrentRegionAndZone = instancesInfo
                                                        .Where(info => info.Value != null && info.Value.RegionId == _regionId && info.Value.ZoneId == _zoneId)
                                                        .ToDictionary(info => info.Value.InstanceId, info => info.Value.UpdateTimestamp);

            if (instancesForCurrentRegionAndZone == null || instancesForCurrentRegionAndZone.Count() == 0)
                return instanceId;

            foreach (var key in instancesForCurrentRegionAndZone.Keys)
            {
                var timeStamp = instancesForCurrentRegionAndZone[key];

                if (DateTime.UtcNow.Subtract(timeStamp) > new TimeSpan(0, 0, elaspedDuration))
                {
                    instanceId = key;
                    isInstanceFree = true;
                    break;
                }
            }

            if (isInstanceFree == false)
            {
                instanceId = instancesForCurrentRegionAndZone.Keys.Max() + 1;
                if (instanceId > 1024)
                    throw new Exception("Instace index out of range. 1024 instance capcacity is already consumed");
            }

            return instanceId;
        }
    }
}
