using System;
using System.Collections.Generic;
using System.Net;

namespace Tavisca.Platform.Common.Models
{
    [Serializable]
    public sealed class ErrorInfo
    {
        public string Code { get; }

        public string Message { get; }

        public HttpStatusCode HttpStatusCode { get; }

        public List<Info> Info { get; private set; }

        public ErrorInfo(string code, string message, HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentNullException(nameof(code));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentNullException(nameof(message));

            Code = code;
            Message = message;
            HttpStatusCode = httpStatusCode;
            Info = new List<Info>();
        }
    }
}
