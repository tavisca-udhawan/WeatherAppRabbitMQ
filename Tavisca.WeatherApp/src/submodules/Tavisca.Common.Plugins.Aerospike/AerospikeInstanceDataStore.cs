using Aerospike.Client;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.LockManagement;
using Tavisca.Platform.Common.Monotone;

namespace Tavisca.Common.Plugins.Aerospike
{
    public class AerospikeInstanceDataStore : IInstanceDataStore
    {
        private readonly NameValueCollection _settings;

        public AerospikeInstanceDataStore(IConfigurationProvider configurationProvider)
        {
            var settings = configurationProvider.GetGlobalConfigurationAsNameValueCollection
                                                            (Keystore.AerospikeKeys.InstanceStore.AppicationName
                                                            , Keystore.AerospikeKeys.InstanceStore.WebfarmSettings
                                                            , Keystore.AerospikeKeys.InstanceStore.AerospikeSettings);

            if (settings == null)
                throw new Exception();

            _settings = settings;
        }

        public Dictionary<string, InstanceInfo> GetInstanceList()
        {
            Dictionary<string, InstanceInfo> instancesInfo = new Dictionary<string, InstanceInfo>();

            var statement = new Statement()
            {
                Namespace = _settings[Keystore.AerospikeKeys.InstanceStore.Namespace],
                SetName = _settings[Keystore.AerospikeKeys.InstanceStore.Set]
            };

            using (AerospikeClient client = GetClient())
            {
                var records = client.Query(null, statement);

                if (records == null)
                    return instancesInfo;

                try
                {
                    while (records.Next())
                    {
                        Key key = records.Key;
                        Record record = records.Record;
                        var instanceInfo = new InstanceInfo()
                        {
                            RegionId = record.GetUInt(Keystore.AerospikeKeys.InstanceStore.RegionIdBin),
                            ZoneId = record.GetUInt(Keystore.AerospikeKeys.InstanceStore.ZoneIdBin),
                            InstanceId = record.GetUInt(Keystore.AerospikeKeys.InstanceStore.InstanceIdBin),
                            UpdateTimestamp = GetTimestamp(record.GetString(Keystore.AerospikeKeys.InstanceStore.TimestampBin))
                        };


                        instancesInfo[instanceInfo.GetUniqueId()] = instanceInfo;

                    }
                }
                finally
                {
                    records.Close();
                }

                return instancesInfo;
            }
        }

        private AerospikeClient GetClient()
        {
            int port = 0;
            var isParsable = int.TryParse(_settings[Keystore.AerospikeKeys.InstanceStore.Port], out port);
            port = isParsable ? port : 3000;
            var aerospikeHosts = new List<Host> { new Host(_settings[Keystore.AerospikeKeys.InstanceStore.Host], port) };
            var secondaryHosts = _settings[Keystore.AerospikeKeys.InstanceStore.SecondaryHosts];
            if (secondaryHosts != null)
            {
                var hosts = secondaryHosts.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                hosts.ForEach(host =>
                                    aerospikeHosts.Add(new Host(host, port)));
            }
            return new AerospikeClient(null, aerospikeHosts.ToArray());
        }

        private DateTime GetTimestamp(string timestampString)
        {
            DateTime timestamp;

            var isparsed = DateTime.TryParse(timestampString, out timestamp);

            return isparsed ? timestamp : DateTime.MinValue;
        }

        public void AddOrUpdate(InstanceInfo instanceInfo)
        {
            if (instanceInfo == null)
                return;

            var policy = new WritePolicy() { recordExistsAction = RecordExistsAction.REPLACE };

            var key = new Key(_settings[Keystore.AerospikeKeys.InstanceStore.Namespace], _settings[Keystore.AerospikeKeys.InstanceStore.Set], instanceInfo.GetUniqueId());

            var regionBin = new Bin(Keystore.AerospikeKeys.InstanceStore.InstanceIdBin, instanceInfo.RegionId);
            var zoneBin = new Bin(Keystore.AerospikeKeys.InstanceStore.InstanceIdBin, instanceInfo.ZoneId);
            var instanceBin = new Bin(Keystore.AerospikeKeys.InstanceStore.InstanceIdBin, instanceInfo.InstanceId);

            var timestampBin = new Bin(Keystore.AerospikeKeys.InstanceStore.TimestampBin, instanceInfo.UpdateTimestamp.ToString("T"));

            using (AerospikeClient client = GetClient())
            {
                client.Put(policy, key, regionBin, zoneBin, instanceBin, timestampBin);
            }
        }


    }
}
