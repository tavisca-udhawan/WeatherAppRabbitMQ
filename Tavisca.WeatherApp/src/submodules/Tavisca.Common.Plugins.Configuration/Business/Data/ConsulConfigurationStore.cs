using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Consul;
using System.Threading;

namespace Tavisca.Common.Plugins.Configuration
{
    public class ConsulConfigurationStore : IConfigurationStore
    {
        public async Task<string> GetAsync(string scope, string application, string section, string key)
        {
            try
            {
                var formatedKey = KeyMaker.ConstructKey(scope, application, section, key).ToLowerInvariant();

                var response = await ConsulClientFactory.Instance.KV.Get(formatedKey);

                return ParseResponse(response);

            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                Platform.Common.ExceptionPolicy.HandleException(ex, Constants.DefaultPolicy);
                throw new ConfigurationException("Could not connect to consul", "100", ex);
            }

            catch (Exception ex)
            {
                Platform.Common.ExceptionPolicy.HandleException(ex, Constants.DefaultPolicy);
            }

            return null;
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

        public string Get(string scope, string application, string section, string key)
        {
            var formatedKey = KeyMaker.ConstructKey(scope, application, section, key).ToLowerInvariant();

            QueryResult<KVPair> response = null;

            var waitHandle = new ManualResetEvent(false);

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    response = await ConsulClientFactory.Instance.KV.Get(formatedKey);
                }
                finally
                {
                    waitHandle.Set();
                }
            });
            waitHandle.WaitOne();

            return ParseResponse(response);
        }

        private string GetString(byte[] value)
        {
            var bomSafeUTF8 = new UTF8Encoding(false);
            return bomSafeUTF8.GetString(value, 0, value.Length);
        }

        public async Task<Dictionary<string, string>> GetAllAsync()
        {
            try
            {
                var dataSet = await ConsulClientFactory.Instance.KV.List("/");
                return ParseResponse(dataSet);

            }
            catch (System.Net.Http.HttpRequestException ex)
            {
                Platform.Common.ExceptionPolicy.HandleException(ex, Constants.DefaultPolicy);
                throw new ConfigurationException("Could not connect to consul", "100", ex);
            }

            catch (Exception ex)
            {
                Platform.Common.ExceptionPolicy.HandleException(ex, Constants.DefaultPolicy);
            }

            return null;
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
    }
}