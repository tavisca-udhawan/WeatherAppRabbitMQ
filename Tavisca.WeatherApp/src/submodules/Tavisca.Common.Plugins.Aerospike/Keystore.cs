namespace Tavisca.Common.Plugins.Aerospike
{
    public static class Keystore
    {
        public static class AerospikeKeys
        {
            public const string SettingsSection = "aerospike_settings";
            public const string LockSettings = "lock_settings";
            public const string StateSettings = "state_settings";
            public const string CounterSettings = "counter_settings";
            public const string SessionProviderSettings = "session_provider_settings";
            public const string Namespace = "namespace";
            public const string Set = "set";

            public static class LuaReferences
            {
                public const string LockPackageName = "LockOperations";
                public const string AcquireReadLock = "tryGetReadLock";
                public const string ReleaseReadLock = "releaseReadLock";
                public const string AcquireWriteLock = "tryGetWriteLock";
                public const string ReleaseWriteLock = "releaseWriteLock";
            }

            public static class BinNames
            {
                public const string ReadLocks = "ReadLocks";
                public const string WriteLocks = "WriteLocks";
                public const string ObjectState = "ObjectState";
                public const string Counter = "Counter";
            }

            public static class Errors
            {
                public const string AerospikeOperationError = "Error occurred in Aerospike provider";
            }

            public static class InstanceStore
            {
                public const string AppicationName = "montone_id_generator";
                public const string AerospikeSettings = "aerospike_settings";
                public const string WebfarmSettings = "webfarm_settings";
                public const string InstanceIdBin = "instance_id";
                public const string ZoneIdBin = "zone_id";
                public const string RegionIdBin = "region_id";
                public const string TimestampBin = "timestamp";
                public const string Namespace = "namespace";
                public const string Set = "set";
                public const string Host = "host";
                public const string SecondaryHosts = "secondaryhosts";
                public const string Port = "port";
            }
        }

    }
}
