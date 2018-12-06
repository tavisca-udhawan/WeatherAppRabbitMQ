using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Tavisca.Platform.Common.Logging;
using Tavisca.Platform.Common.Models;

namespace Tavisca.Platform.Common.WebApi.Filter
{
    public abstract class ExceptionLoggingFilterAttribute : ExceptionFilterAttribute
    {
        public static readonly string DefaultPolicy = "default";

        private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);
        public sealed override async Task OnExceptionAsync(HttpActionExecutedContext context, CancellationToken cancellationToken)
        {
            await SetExceptionDataAsync(context.Exception, context, cancellationToken);
            await HandleFaultAsync(context, context.Exception, cancellationToken);
        }

        private async Task HandleFaultAsync(HttpActionExecutedContext context, Exception ex, CancellationToken token)
        {
            Exception newEx;
            // Log the error.
            ExceptionPolicy.HandleException(ex, DefaultPolicy, out newEx);
            // Translate to generic error for exception shielding
            var appEx = newEx as BaseApplicationException;
            string  stringResponse;
            if (appEx != null)
            {
                stringResponse = GetContent(ToErrorInfo(appEx));
                context.Response = new HttpResponseMessage(appEx.HttpStatusCode)
                {
                    Content = new StringContent(stringResponse, Utf8WithoutBom, "application/json")
                };
            }
            else
            {
                stringResponse = GetContent(GetInternalServerError(context));
                context.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent(stringResponse, Utf8WithoutBom, "application/json")
                };
            }
        }

        private string GetContent(ErrorInfo errorInfo)
        {
            return JsonConvert.SerializeObject(errorInfo, new ErrorInfoTranslator(), new InfoTranslator());
        }

        public abstract ErrorInfo GetInternalServerError(HttpActionExecutedContext context);

        public abstract Task SetExceptionDataAsync(Exception exception, HttpActionExecutedContext context, CancellationToken token);

        private static ErrorInfo ToErrorInfo(BaseApplicationException exception)
        {
            var errorInfo = new ErrorInfo(exception.ErrorCode, exception.ErrorMessage, exception.HttpStatusCode);
            if (exception.Info != null && exception.Info.Count > 0)
                errorInfo.Info.AddRange(exception.Info);
            return errorInfo;
        }
    }
}