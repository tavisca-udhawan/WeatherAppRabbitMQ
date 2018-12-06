using System;
using System.Runtime.Serialization;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    [Serializable]
    public class LoggingException : Exception
    {
        public LoggingException() : base()
        {
        }

        public LoggingException(string message) : base(message)
        {
        }

        public LoggingException(string message, Exception innerException) : base(message, innerException)
        {
        }
#if !NET_STANDARD
        public LoggingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}
