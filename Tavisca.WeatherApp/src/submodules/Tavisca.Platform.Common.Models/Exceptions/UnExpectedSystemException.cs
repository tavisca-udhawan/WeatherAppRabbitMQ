
using System;
using System.Net;
using Tavisca.Platform.Common.Models;
using System.Runtime.Serialization;

namespace Tavisca.Platform.Common.Models
{
    [Serializable]
    public partial class UnExpectedSystemException : BaseApplicationException
    {
        public UnExpectedSystemException(string code, string message, HttpStatusCode httpStatusCode) : base(code, message, httpStatusCode) { }
#if !NET_STANDARD
        public UnExpectedSystemException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
        public UnExpectedSystemException(ErrorInfo info) : base(info.Code, info.Message, info.HttpStatusCode, info.Info) { }

        public UnExpectedSystemException(string message, string code, Exception inner) : base(message, code, inner) { }
    }
}
