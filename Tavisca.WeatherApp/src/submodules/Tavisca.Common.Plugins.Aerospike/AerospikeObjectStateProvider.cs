using Aerospike.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Profiling;
using Tavisca.Platform.Common.StateManagement;
using Tavisca.Frameworks.Serialization;

namespace Tavisca.Common.Plugins.Aerospike
{
    public class AerospikeObjectStateProvider : IStateProvider
    {

        private AsyncClient GetClient(AerospikeSettings settings)
        {
            return _clientFactory.GetClient(settings.Host, settings.Port, settings.SecondaryHosts);

        }

        readonly IStateProviderSettings _settingsProvider;
        readonly IAerospikeClientFactory _clientFactory;
        private static ISerializationFacade _serializationFacade;
        static AerospikeObjectStateProvider()
        {
            _serializationFacade = new SerializationFactory().GetSerializationFacade();
        }
        public AerospikeObjectStateProvider(IAerospikeClientFactory clientFactory, IStateProviderSettings settingsProvider)
        {
            _clientFactory = clientFactory;
            _settingsProvider = settingsProvider;
        }

        public async Task<bool> Exists(string stateId, CancellationToken cancellationToken)
        {
            var settings = await _settingsProvider.GetSettings();
            var recordKey = GetRecordKey(settings, stateId);
            var client = GetClient(settings);
            if (client.Exists(null, recordKey) == true)
                return true;
            recordKey = GetAlertnateRecordKey(settings, stateId);
            return client.Exists(null, recordKey);
        }
        public async Task<object> GetStateAsync(string stateId, CancellationToken cancellationToken)
        {
            object stateValue = null;
            try
            {
                var settings = await _settingsProvider.GetSettings();
                var recordKey = GetRecordKey(settings, stateId);

                var client = GetClient(settings);
                var record = await client.Get(null, cancellationToken, recordKey);

                if (record == null || !record.bins.TryGetValue(Keystore.AerospikeKeys.BinNames.ObjectState, out stateValue))
                {
                    recordKey = GetAlertnateRecordKey(settings, stateId);
                    record = await client.Get(null, cancellationToken, recordKey);
                    if (record == null || !record.bins.TryGetValue(Keystore.AerospikeKeys.BinNames.ObjectState, out stateValue))
                        return null;
                }
            }
            catch (Exception ex)
            {
                throw new AerospikeProviderException(Keystore.AerospikeKeys.Errors.AerospikeOperationError, ex);
            }
            using (new ProfileContext("State deserialization"))
                return _serializationFacade.Deserialize<object>((byte[])stateValue);
        }

        public async Task SaveStateAsync(string stateId, object state, CancellationToken cancellationToken, int ExpiryTime = 3600)
        {
            if (string.IsNullOrWhiteSpace(stateId))
                throw new ArgumentNullException(nameof(stateId));
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            byte[] serializedValue;
            using (new ProfileContext("State serialization"))
                serializedValue = _serializationFacade.Serialize(state);
            try
            {
                var settings = await _settingsProvider.GetSettings();
                var recordKey = GetRecordKey(settings, stateId);

                var bin = new Bin(Keystore.AerospikeKeys.BinNames.ObjectState, serializedValue);
                var client = GetClient(settings);
                await client.Put(GetRecordPolicy(ExpiryTime), cancellationToken, recordKey, bin);
            }
            catch (Exception ex)
            {
                throw new AerospikeProviderException(Keystore.AerospikeKeys.Errors.AerospikeOperationError, ex);
            }
        }



        #region private methods
        private WritePolicy GetRecordPolicy(int ExpiryTime)
        {

            return new WritePolicy()
            {
                expiration = ExpiryTime,
                sendKey = true
            };
        }

        private Key GetRecordKey(AerospikeSettings settings, string stateId)
        {

            return new Key(settings.Namespace, settings.Set, Value.Get(stateId));
        }

        private Key GetAlertnateRecordKey(AerospikeSettings settings, string stateId)
        {

            return new Key(settings.SecondaryNamespace, settings.Set, Value.Get(stateId));
        }

        #endregion
    }
}
