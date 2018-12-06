using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Cors;
using System.Web.Http.Cors;

namespace Tavisca.Platform.Common.WebApi.Cors
{
    public class OskiCorsPolicy : ICorsPolicyProvider
    {
        private readonly CorsPolicy _policy;

        private static readonly List<string> Headers =
            new List<string>
            {
                HeaderNames.CorrelationId,
                HeaderNames.ContentType,
                HeaderNames.AcceptType,
                HeaderNames.AcceptLanguage ,
                HeaderNames.TenantId,
                HeaderNames.UserToken,
                HeaderNames.IpAddress
                };


        private static readonly List<string> Methods = new List<string>() { "PUT","POST","GET","DELETE","PATCH","OPTIONS" };
        public OskiCorsPolicy()
        {
            // Create a CORS policy.
            _policy = new CorsPolicy
            {
                AllowAnyOrigin = true,
                PreflightMaxAge = 86400
            };

            // Add allowed origins.
            Headers.ForEach(header => _policy.Headers.Add(header));
            Methods.ForEach(method => _policy.Methods.Add(method));
        }

        public Task<CorsPolicy> GetCorsPolicyAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_policy);
        }
    }
}
