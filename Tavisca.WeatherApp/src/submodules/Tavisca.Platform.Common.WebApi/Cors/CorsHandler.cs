using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Tavisca.Platform.Common.WebApi.Cors
{
    public class CorsHandler : DelegatingHandler
    {
        private const string Origin = "*";

        private static readonly string Headers =
            HeaderNames.CorrelationId + "," +
            HeaderNames.UserToken + "," +
            HeaderNames.ContentType + "," +
            HeaderNames.AcceptType + "," +
            HeaderNames.AcceptLanguage + "," +
            HeaderNames.TenantId + "," +
            HeaderNames.IpAddress;


        private const string Methods = "PUT,POST,GET,DELETE,PATCH,OPTIONS";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {

            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                return Task.Factory.StartNew(() =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);

                    response.Headers.Add(HeaderNames.AccessControlAllowOrigin, Origin);
                    response.Headers.Add(HeaderNames.AccessControlAllowMethods, Methods);
                    response.Headers.Add(HeaderNames.AccessControlAllowHeaders, Headers);
                    return response;
                },cancellationToken);
            }

            return base.SendAsync(request, cancellationToken).ContinueWith(task =>
            {
                var response = task.Result;
                response.Headers.Add(HeaderNames.AccessControlAllowOrigin, Origin);
                return response;
            });
        }
    }
}
