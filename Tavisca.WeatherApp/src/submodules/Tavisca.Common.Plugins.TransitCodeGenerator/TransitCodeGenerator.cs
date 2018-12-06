using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Threading.Tasks;
using Tavisca.Platform.Common;
using Tavisca.Platform.Common.Plugins.Json;

namespace Tavisca.Common.Plugins.TransitCodeGenerator
{
    public class TransitCodeGenerator
    {
        
        public async Task<TransitCodeResponse> GenerateTransitCode(TransitCodeRequest transitCodeRequest, string url)
        {
            TransitCodeRequestValidator.Validate(transitCodeRequest);
            HttpResponse webResponse = null;
            var webrequest = await new WebCaller().GetWebRequest(new { transitCodeRequest }, url);
            try
            {
                webResponse = await webrequest.SendAsync();
            }
            catch (Exception ex)
            {
                return new TransitCodeResponse
                {
                    Status = "failed",
                    Message = ex.Message
                };
            }

            var response = await webResponse.GetResponseAsync<TransitCodeResponse>();
            return response;
        }
       
    }
}
