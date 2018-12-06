
using System;
using System.Net;
using Tavisca.Platform.Common.Models;
using System.Runtime.Serialization;

namespace Tavisca.Platform.Common.Models
{
    [Serializable]
    public partial class CommunicationException : BaseApplicationException
    {
        public CommunicationException(string code, string message, HttpStatusCode httpStatusCode) : base(code, message, httpStatusCode) { }
#if !NET_STANDARD
        public CommunicationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif
        public CommunicationException(ErrorInfo info) : base(info.Code, info.Message, info.HttpStatusCode) { }
    }
}
