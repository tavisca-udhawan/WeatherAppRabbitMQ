using Aerospike.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.LockManagement;

namespace Tavisca.Common.Plugins.Aerospike
{
    public class AerospikeLockProvider : ILockProvider
    {
      
        private AsyncClient GetClient(AerospikeSettings settings)
        {
            return   _clientFactory.GetClient(settings.Host, settings.Port, settings.SecondaryHosts);
        }

        readonly ILockProviderSettings _settingsProvider;
        readonly IAerospikeClientFactory _clientFactory;
        public AerospikeLockProvider(IAerospikeClientFactory clientFactory, ILockProviderSettings settingsProvider)
        {
            _clientFactory = clientFactory;
            _settingsProvider = settingsProvider;
        }
        public async Task<bool> TryGetLockAsync(string lockId, LockType lockType, CancellationToken cancellationToken)
        {
            try
            {
                var settings = await _settingsProvider.GetSettings();
                var lockFunction = GetLockAcquireFunction(lockType);
                var recordKey = await GetRecordKey(settings, lockId);
                 CreateRecordIfMissing(settings, recordKey);

                var client =  GetClient(settings);
                var status = client.Execute(GetUdfExecutionPolicy(), recordKey,
                     Keystore.AerospikeKeys.LuaReferences.LockPackageName, lockFunction,
                     Value.Get(Keystore.AerospikeKeys.BinNames.ReadLocks), Value.Get(Keystore.AerospikeKeys.BinNames.WriteLocks));

                return ((long)status) > 0;
            }
            catch (Exception ex)
            {
                throw new AerospikeProviderException(Keystore.AerospikeKeys.Errors.AerospikeOperationError, ex);
            }
        }

        public async Task ReleaseLockAsync(string lockId, LockType lockType, CancellationToken cancellationToken)
        {
            try
            {
                var settings = await _settingsProvider.GetSettings();
                var lockFunction = GetLockReleaseFunction(lockType);
                var recordKey = await GetRecordKey(settings,lockId);

                var binName = lockType == LockType.Read ? Keystore.AerospikeKeys.BinNames.ReadLocks
                    : Keystore.AerospikeKeys.BinNames.WriteLocks;
                 CreateRecordIfMissing(settings,recordKey);

                var client =  GetClient(settings);
                client.Execute(GetUdfExecutionPolicy(), recordKey,
                        Keystore.AerospikeKeys.LuaReferences.LockPackageName, lockFunction, Value.Get(binName));
            }
            catch (Exception ex)
            {
                throw new AerospikeProviderException(Keystore.AerospikeKeys.Errors.AerospikeOperationError, ex);
            }
        }

        #region private methods

        private string GetLockAcquireFunction(LockType lockType)
        {
            return lockType == LockType.Read ? Keystore.AerospikeKeys.LuaReferences.AcquireReadLock
                : Keystore.AerospikeKeys.LuaReferences.AcquireWriteLock;
        }

        private string GetLockReleaseFunction(LockType lockType)
        {
            return lockType == LockType.Read ? Keystore.AerospikeKeys.LuaReferences.ReleaseReadLock
                : Keystore.AerospikeKeys.LuaReferences.ReleaseWriteLock;
        }

        internal async Task<Key> GetRecordKey(AerospikeSettings settings, string stateId)
        {
            return new Key(settings.Namespace, settings.Set, Value.Get(stateId));
        }

        private void CreateRecordIfMissing(AerospikeSettings settings,Key recordKey)
        {
            var client =  GetClient(settings);
            if (client.Exists(null, new Key[] { recordKey }[0]))
                return;

            var writePolicy = new WritePolicy()
            {
                recordExistsAction = RecordExistsAction.CREATE_ONLY,
                expiration = 3600,
                sendKey = true
            };
            try
            {
                client.Put(writePolicy, recordKey, new Bin[2] {
                    new Bin(Keystore.AerospikeKeys.BinNames.ReadLocks, 0),
                    new Bin(Keystore.AerospikeKeys.BinNames.WriteLocks, 0)
                });
            }
            catch(AerospikeException aex)
            {
                if (aex.Result != 5)
                    throw;
            }
        }

        private WritePolicy GetUdfExecutionPolicy()
        {
            return new WritePolicy()
            {
                recordExistsAction = RecordExistsAction.UPDATE_ONLY
            };
        }

        #endregion
    }
}
