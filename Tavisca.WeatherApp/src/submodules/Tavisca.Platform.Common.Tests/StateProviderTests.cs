using Aerospike.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.Aerospike;
using Tavisca.Common.Plugins.Configuration;
using Xunit;

namespace Tavisca.Platform.Common.Tests
{
    public class StateProviderTests
    {
        [Fact(Skip="Call aerospike host which is not reachable")]
        public async Task TestStateSaveAndRetrieval()
        {
            var stateId = new Random().Next().ToString();
            var stateProvider = new AerospikeObjectStateProvider(GetClientFactory(), GetSettingsProvider());
            try
            {
                await stateProvider.SaveStateAsync(stateId, new SampleState() { Data = "Kratos" }, CancellationToken.None);
                var state = await stateProvider.GetStateAsync(stateId, CancellationToken.None) as SampleState;

                Assert.NotNull(state);
                Assert.Equal(state.Data, "Kratos");
                Assert.False(state.Status);
                state.Data = "Poseidon";
                state.Status = true;
                await stateProvider.SaveStateAsync(stateId, state, CancellationToken.None);
                state = await stateProvider.GetStateAsync(stateId, CancellationToken.None) as SampleState;

                Assert.NotNull(state);
                Assert.True(state.Status);
                Assert.Equal(state.Data, "Poseidon");
            }
            finally
            {
                var client = await GetClient();
                client.Delete(null, await GetRecordKey(stateId));
            }
        }

        private static async Task<Key> GetRecordKey(string userKey)
        {
            var settings = await GetSettingsProvider().GetSettings();
            return new Key(settings.Namespace, settings.Set, userKey);
        }

        private static IStateProviderSettings _settingsProvider;
        private static IStateProviderSettings GetSettingsProvider()
        {
            return _settingsProvider ?? (_settingsProvider = new AerospikeSettingsProvider(new ConfigurationProvider("test_application")));
        }

        private static IAerospikeClientFactory GetClientFactory()
        {
            return new AerospikeClientFactory();
        }

        private static async Task<AsyncClient> GetClient()
        {
            var settings = await GetSettingsProvider().GetSettings();
            return  GetClientFactory().GetClient(settings.Host, settings.Port, settings.SecondaryHosts);
        }
    }

    [Serializable]
    public class SampleState
    {
        public string Data { get; set; }
        public bool Status { get; set; }
    }
}
