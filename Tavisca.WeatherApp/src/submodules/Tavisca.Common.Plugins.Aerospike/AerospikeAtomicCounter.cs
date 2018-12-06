using Aerospike.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Counter;

namespace Tavisca.Common.Plugins.Aerospike
{
    public class AerospikeAtomicCounter : IAtomicCounter
    {

        private AsyncClient GetClient(AerospikeSettings settings)
        {            
            var client =  _clientFactory.GetClient(settings.Host, settings.Port,settings.SecondaryHosts);
            return client;
        }

        readonly ICounterSettings _settingsProvider;
        readonly IAerospikeClientFactory _clientFactory;
        public AerospikeAtomicCounter(IAerospikeClientFactory clientFactory, ICounterSettings settingsProvider)
        {
            _clientFactory = clientFactory;
            _settingsProvider = settingsProvider;
        }


        public async Task Create(string id, TimeSpan expiry, long defaultValue = 0, CancellationToken cancellationToken = default(CancellationToken))
        {
           

            var writePolicy = new WritePolicy()
            {
                recordExistsAction = RecordExistsAction.CREATE_ONLY,
                expiration = (int)expiry.TotalSeconds
            };
            try
            {
                var settings = await _settingsProvider.GetSettings();

                var client = GetClient(settings);
                var recordKey = GetRecordKey(settings, id);

                await client.Put(writePolicy, cancellationToken, recordKey, new Bin[1] {
                    new Bin(Keystore.AerospikeKeys.BinNames.Counter, Value.Get(defaultValue))
                });
            }
            catch (AerospikeException aex)
            {
                throw new AerospikeProviderException(Keystore.AerospikeKeys.Errors.AerospikeOperationError, aex);
            }
        }

        public async Task<long> GetCurrentValue(string id, CancellationToken cancellationToken = default(CancellationToken))
        {
          

            try
            {
                var settings = await _settingsProvider.GetSettings();
                var client = GetClient(settings);
                var recordKey = GetRecordKey(settings, id);

                var binName = Keystore.AerospikeKeys.BinNames.Counter;
                var record = await client.Get(null, cancellationToken, recordKey, binName);
                return record.GetLong(binName);
            }
            catch (AerospikeException aex)
            {
                throw new AerospikeProviderException("Failed getting current counter value", aex);
            }
        }

        public async Task<long> Increment(string id, int incrementFactor = 1, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var settings = await _settingsProvider.GetSettings();
                var client =  GetClient(settings);
                var recordKey =  GetRecordKey(settings,id);
                var binName = Keystore.AerospikeKeys.BinNames.Counter;
                var record = await client.Operate(null, cancellationToken, recordKey, Operation.Add(new Bin(binName, incrementFactor)), Operation.Get(binName));
                return record.GetLong(binName);
            }
            catch (Exception ex)
            {
                throw new AerospikeProviderException(Keystore.AerospikeKeys.Errors.AerospikeOperationError, ex);
            }
        }

        public async Task<long> Decrement(string id, int decrementFactor = 1, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var settings = await _settingsProvider.GetSettings();
                var client =  GetClient(settings);
                var recordKey =  GetRecordKey(settings,id);
                var binName = Keystore.AerospikeKeys.BinNames.Counter;
                var record = await client.Operate(null, cancellationToken, recordKey, Operation.Add(new Bin(binName, -decrementFactor)), Operation.Get(binName));
                return record.GetLong(binName);
            }
            catch (Exception ex)
            {
                throw new AerospikeProviderException(Keystore.AerospikeKeys.Errors.AerospikeOperationError, ex);
            }
        }

        #region private methods


        internal Key GetRecordKey(AerospikeSettings settings,string counterId)
        {
            return new Key(settings.Namespace, settings.Set, Value.Get(counterId));
        }

        #endregion
    }
}
