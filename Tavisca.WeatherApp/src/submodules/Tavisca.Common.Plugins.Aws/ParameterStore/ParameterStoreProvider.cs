using Amazon;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.ConfigurationHandler;

namespace Tavisca.Common.Plugins.Aws
{
    public class ParameterStoreProvider : ISensitiveDataProvider
    {
        private AmazonSimpleSystemsManagementClient _client;

        public async Task<Dictionary<string, string>> GetValuesAsync(List<string> keys, bool decryptionRequired = true)
        {
            var keyValuePair = new Dictionary<string, string>();

            if (_client == null)
                _client = GetClient();

            try
            {
                var validParameters = new List<Parameter>();
                var invalidParameters = new List<string>();

                var keyBatches = GetKeyBatches(keys);

                foreach (var batch in keyBatches)
                {
                    var response = await GetParametersResponse(batch, decryptionRequired);
                    validParameters.AddRange(response.Parameters);
                    invalidParameters.AddRange(response.InvalidParameters);
                }

                validParameters.ForEach(p =>
                {
                    keyValuePair.Add(p.Name, p.Value);
                });

                invalidParameters.ForEach(p =>
                {
                    keyValuePair.Add(p, null);
                });

                if (invalidParameters.Count > 0)
                {
                    throw Platform.Common.Models.Errors.ClientSide.MissingKeysInParameterStore(invalidParameters);
                }
            }
            catch (Exception ex)
            {
                Platform.Common.ExceptionPolicy.HandleException(ex, Constants.LogOnlyPolicy);
            }

            return keyValuePair;
        }

        private AmazonSimpleSystemsManagementClient GetClient()
        {
            try
            {
                var region = ConfigurationManager.GetAppSetting("ParameterStore.Region");
                var regionEndpoint = string.IsNullOrWhiteSpace(region) ? RegionEndpoint.USEast1 : RegionEndpoint.GetBySystemName(region);

                return new AmazonSimpleSystemsManagementClient(regionEndpoint);
            }
            catch (Exception ex)
            {
                Platform.Common.ExceptionPolicy.HandleException(ex, Constants.LogOnlyPolicy);
                throw Platform.Common.Models.Errors.ServerSide.ParameterStoreCommunicationError();
            }
        }

        private List<List<string>> GetKeyBatches(List<string> keys)
        {
            var keyBatches = new List<List<string>>();

            for (int i = 0; i < keys.Count; i += 10)
            {
                keyBatches.Add(keys.GetRange(i, Math.Min(10, keys.Count - i)));
            }

            return keyBatches;
        }

        private async Task<GetParametersResponse> GetParametersResponse(List<string> keys, bool decryptionRequired)
        {
            var request = new GetParametersRequest
            {
                Names = keys,
                WithDecryption = decryptionRequired
            };

            return await _client.GetParametersAsync(request);
        }
    }
}