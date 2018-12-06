
using System;
using System.Net;
using Tavisca.Platform.Common.Models;
using System.Runtime.Serialization;

namespace Tavisca.Common.Plugins.Configuration
{
    [Serializable]
    public partial class SignalRConnectionException : BaseApplicationException
    {
        public SignalRConnectionException(string code, string message, HttpStatusCode httpStatusCode) : base(code, message, httpStatusCode) { }

        public SignalRConnectionException(SerializationInfo info,StreamingContext context) : base(info,context) {}

		public SignalRConnectionException(ErrorInfo info) : base(info.Code, info.Message, info.HttpStatusCode) { }

		public SignalRConnectionException(string message, string code, Exception inner) : base(message, code, inner){ }
    }
}
