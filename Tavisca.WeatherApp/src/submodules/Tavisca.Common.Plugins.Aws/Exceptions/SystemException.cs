
using System;
using System.Net;
using Tavisca.Platform.Common.Models;
using System.Runtime.Serialization;

namespace Tavisca.Common.Plugins.Aws
{
    [Serializable]
    public partial class SystemException : BaseApplicationException
    {
        public SystemException(string code, string message, HttpStatusCode httpStatusCode) : base(code, message, httpStatusCode) { }

		public SystemException(ErrorInfo info) : base(info.Code, info.Message, info.HttpStatusCode,info.Info) { }

		public SystemException(string message, string code, Exception inner) : base(message, code, inner){ }
    }
}
