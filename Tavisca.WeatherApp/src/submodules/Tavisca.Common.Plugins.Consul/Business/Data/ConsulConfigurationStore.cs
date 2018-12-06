using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Consul;
using System.Threading;
using Tavisca.Platform.Common.Logging;
using System.Configuration;
using Tavisca.Platform.Common.Plugins.Json;
using Tavisca.Platform.Common.Profiling;
using Tavisca.Platform.Common.Context;

namespace Tavisca.Common.Plugins.Configuration
{
    public class ConsulConfigurationStore : IConfigurationStore
    {
        private const string _applicationName = "consul_configuration_store";

        public async Task<string> GetAsync(string scope, string application, string section, string key)
        {

            try
            {
                using (new ProfileContext("ConsulConfigurationStore.Get"))
                {
                    var formatedKey = KeyMaker.ConstructKey(scope, application, section, key).ToLowerInvariant();
                    Profiling.Trace($"Key: {formatedKey}");
                    var response = await ConsulClientFactory.Instance.KV.Get(formatedKey);
                    await LogRQRS(formatedKey, response, application, "get_async");

                    return ParseResponse(response);
                }
            }

            catch (Exception ex)
            {
                Platform.Common.ExceptionPolicy.HandleException(ex, Constants.DefaultPolicy);
            }
            throw new ConfigurationException("Could not connect to consul", "100", System.Net.HttpStatusCode.InternalServerError);
        }

        public async Task<Dictionary<string, string>> GetAllAsync()
        {
            try
            {
                using (new ProfileContext("ConsulConfigurationStore.GetAll"))
                {
                    var formatedKey = "/";
                    var dataSet = await ConsulClientFactory.Instance.KV.List(formatedKey);

                    await LogRQRS(formatedKey, dataSet, _applicationName, "get_all_async");
                    var response = ParseResponse(dataSet);
                    if (response != null)
                        Profiling.Trace($"Key: {response.Count}");
                    return response;
                }

            }


            catch (Exception ex)
            {
                Platform.Common.ExceptionPolicy.HandleException(ex, Constants.DefaultPolicy);
            }

            throw new ConfigurationException("Could not connect to consul", "100", System.Net.HttpStatusCode.InternalServerError);
        }

        public string Get(string scope, string application, string section, string key)
        {
            using (new ProfileContext("ConsulConfigurationStore.Get"))
            {
                var formatedKey = KeyMaker.ConstructKey(scope, application, section, key).ToLowerInvariant();

                Profiling.Trace($"Key: {formatedKey}");

                QueryResult<KVPair> response = null;

                var waitHandle = new ManualResetEvent(false);

                Task.Factory.StartNew(async () =>
                {
                    try
                    {
                        response = await ConsulClientFactory.Instance.KV.Get(formatedKey);
                        await LogRQRS(formatedKey, response, application, "get");
                    }
                    finally
                    {
                        waitHandle.Set();
                    }
                });
                waitHandle.WaitOne();

                return ParseResponse(response);
            }
        }

        public async Task<ConfigurationStoreConnectionStatus> HealthCheckAsync()
        {
            try
            {
                var response = await ConsulClientFactory.Instance.Status.Peers();
                if (response?.Length == 0) throw new ConfigurationException("Could not connect to consul", "100", System.Net.HttpStatusCode.InternalServerError);
                return ConfigurationStoreConnectionStatus.Connected;

            }
            catch (Exception ex)
            {
                Platform.Common.ExceptionPolicy.HandleException(ex, Constants.LogOnlyPolicy);
                return ConfigurationStoreConnectionStatus.Disconnected;
            }

        }

        private async Task LogRQRS(string formatedKey, QueryResult<KVPair> response, string applicationName, string verb)
        {
            if (response != null && response.Response != null)
                await LogRQRS(formatedKey, new QueryResult<KVPair[]> { Response = new KVPair[1] { response.Response } }, applicationName, verb);
        }

        private async Task LogRQRS(string formatedKey, QueryResult<KVPair[]> response, string applicationName, string verb)
        {
            try
            {
                bool logValues = false;
                var debugMode = ConfigurationManager.AppSettings["consul-debug"];
                if (string.IsNullOrWhiteSpace(debugMode) == false && debugMode.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                    logValues = true;

                var responseCopy = from kvPair in response.Response
                                   select new KVPair(kvPair.Key) { Value = logValues ? kvPair.Value : null };
                var context = CallContext.Current;

                var log = new ApiLog
                {
                    ApplicationName = applicationName ?? _applicationName,
                    Verb = verb,
                    Request = new Payload(formatedKey),
                    Response = new Payload(ByteHelper.ToByteArrayUsingJsonSerialization(responseCopy)),
                    ApplicationTransactionId = context?.TransactionId,
                    CorrelationId = context?.CorrelationId,
                    StackId = context?.StackId,
                    TenantId = context?.TenantId,
                    Api = "consul"
                };

                if (response != null && response.Response != null)
                    log.IsSuccessful = true;

                await Logger.WriteLogAsync(log);
            }
            catch (Exception ex)
            {
                Platform.Common.ExceptionPolicy.HandleException(ex, Constants.LogOnlyPolicy);
            }
        }

        private string ParseResponse(QueryResult<KVPair> response)
        {
            if (response == null || response.Response == null)
                return null;

            var value = response.Response.Value;

            if (value != null && value.Length > 0)
            {

                var valueString = GetString(value);

                return valueString;
            }

            return null;
        }

        private Dictionary<string, string> ParseResponse(QueryResult<KVPair[]> response)
        {
            if (response == null || response.Response == null || response.Response.Length == 0)
                return null;

            var kvStore = new Dictionary<string, string>();
            foreach (var item in response.Response)
            {
                var value = item.Value;

                if (value != null && value.Length > 0)
                {
                    var valueString = GetString(value);
                    kvStore.Add(item.Key, valueString);

                }

            }
            return kvStore;

        }

        private string GetString(byte[] value)
        {
            var bomSafeUTF8 = new UTF8Encoding(false);
            return bomSafeUTF8.GetString(value, 0, value.Length);
        }


    }
}