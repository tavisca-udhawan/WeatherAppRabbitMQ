
using System;
using System.Net;
using Tavisca.Platform.Common.Models;
using System.Runtime.Serialization;

namespace Tavisca.Platform.Common.Models
{
    [Serializable]
    public partial class ConfigurationException : BaseApplicationException
    {
        public ConfigurationException(string code, string message, HttpStatusCode httpStatusCode) : base(code, message, httpStatusCode) { }
#if !NET_STANDARD
        public ConfigurationException(SerializationInfo info,StreamingContext context) : base(info,context) {}
#endif
        public ConfigurationException(ErrorInfo info) : base(info.Code, info.Message, info.HttpStatusCode) { }
    }
}
