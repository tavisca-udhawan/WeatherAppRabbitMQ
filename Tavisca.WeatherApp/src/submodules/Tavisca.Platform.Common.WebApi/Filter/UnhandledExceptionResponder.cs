using System;
using System.Net;
using System.Net.Http;
using Tavisca.Platform.Common.Models;

namespace Tavisca.Platform.Common.WebApi
{
    public class UnhandledExceptionResponderAttribute : UnhandledExceptionFilterAttribute
    {

        public UnhandledExceptionResponderAttribute(LogHandler logHandler)
        {
            this.OnLogging += logHandler;
        }

        public UnhandledExceptionResponderAttribute()
        {
        }

        public override ErrorInfo GetUnhandledErrorInfoObject(HttpRequestMessage request, Exception ex)
        {
            Exception newException;
            ExceptionPolicy.HandleException(ex, ExceptionManagement.Policies.DefaultPolicy, out newException);
            var errorCodeAwareException = newException as BaseApplicationException;
            if (errorCodeAwareException != null)
            {
                return ToErrorInfo(errorCodeAwareException);
            }
            return GetGenericErrorResponse();
        }

        private static ErrorInfo GetGenericErrorResponse()
        {
            return new ErrorInfo(FaultCodes.UnexpectedSystemException, ErrorMessages.UnexpectedSystemException());
        }

        public override HttpStatusCode GetHttpStatusCode(Exception ex)
        {
            var errorCodeAwareException = ex as BaseApplicationException;
            if (errorCodeAwareException != null)
                return errorCodeAwareException.HttpStatusCode;

            return HttpStatusCode.InternalServerError;
        }

        private static ErrorInfo ToErrorInfo(BaseApplicationException exception)
        {
            var errorInfo = new ErrorInfo(exception.ErrorCode, exception.ErrorMessage, exception.HttpStatusCode);
            if (exception.Info != null && exception.Info.Count > 0)
                errorInfo.Info.AddRange(exception.Info);
            return errorInfo;
        }
    }
}