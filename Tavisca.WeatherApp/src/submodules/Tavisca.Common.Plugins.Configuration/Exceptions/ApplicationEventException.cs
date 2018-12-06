
using System;
using System.Net;
using System.Runtime.Serialization;
using Tavisca.Platform.Common.Models;

namespace Tavisca.Common.Plugins.Configuration
{
    [Serializable]
    public partial class ApplicationEventException : BaseApplicationException
    {
        public ApplicationEventException(string code, string message, HttpStatusCode httpStatusCode) : base(code, message, httpStatusCode) { }

        public ApplicationEventException(ErrorInfo info) : base(info.Code, info.Message, info.HttpStatusCode) { }

        public ApplicationEventException(string message, string code, Exception inner) : base(message, code, inner) { }
    }
}
