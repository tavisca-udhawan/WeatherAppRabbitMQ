using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Web.Http.Filters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Tavisca.Platform.Common.Models;

namespace Tavisca.Platform.Common.WebApi
{
    public class LogEventArgs : EventArgs
    {
        public HttpRequestMessage Request { get; set; }
        public Exception Exception { get; set; }
    }

    public abstract class UnhandledExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private static readonly Encoding UTF8WithoutBom = new UTF8Encoding(false);
        public sealed override void OnException(HttpActionExecutedContext context)
        {
            if (OnLogging != null)
                OnLogging.Invoke(this, new LogEventArgs() { Exception = context.Exception, Request = context.Request });

            context.Response = new HttpResponseMessage(GetHttpStatusCode(context.Exception));
            context.Response.Content =
                new StringContent(ConvertToDataContract(GetUnhandledErrorInfoObject(context.Request, context.Exception)),
                UTF8WithoutBom, "application/json");
        }

        public delegate void LogHandler(object sender, LogEventArgs e);

        public event LogHandler OnLogging;
        public abstract ErrorInfo GetUnhandledErrorInfoObject(HttpRequestMessage request, Exception ex);
        public abstract HttpStatusCode GetHttpStatusCode(Exception ex);

        private string ConvertToDataContract(ErrorInfo errorInfo)
        {
            return JsonConvert.SerializeObject(errorInfo, new ErrorInfoTranslator(), new InfoTranslator());

        }

    }
}