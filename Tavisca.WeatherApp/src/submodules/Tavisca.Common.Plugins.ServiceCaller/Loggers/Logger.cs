using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Tavisca.Common.Plugins.WebClient;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    public class Logger
    {
        public delegate Task LogHandler(object sender, LogEventArgs e);
        internal event LogHandler OnLogging;
        public Logger(LogHandler logHandler)
        {
            OnLogging += logHandler;
        }
        internal bool Validate()
        {
            if (OnLogging == null)
                return false;
            return true;
        }
        public async Task RaiseLogEvent(object request, object response, string requestString, string responseString, double timeTakenInSeconds, WebClientResponseMessage webClientResponseMessage, NameValueCollection requestHeaders, string url, DateTime startTimestamp)
        {
            if (OnLogging != null)
            {
                var eventArgs = new LogEventArgs()
                {
                    RequestString = requestString,
                    Request = request,
                    ResponseString = responseString,
                    Response = response,
                    TimeTakenInSeconds = timeTakenInSeconds,
                    RequestHeaders = requestHeaders, 
                    Url = url,
                    Timestamp = startTimestamp
                };

                if (webClientResponseMessage != null)
                {
                    eventArgs.HttpStatusCode = webClientResponseMessage.HttpStatusCode;
                    eventArgs.ContentHeaders = webClientResponseMessage.ContentHeaders;
                    eventArgs.ResponseHeaders = webClientResponseMessage.ResponseHeaders;
                }
                await OnLogging(null, eventArgs);
            }
        }

    }
}
