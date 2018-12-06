using System;
using System.Runtime.Serialization;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    [Serializable]
    public class SerializationException : Exception
    {
        public SerializationException() : base()
        {
        }
        public SerializationException(string message) : base(message)
        {
        }
        public SerializationException(Type type, Exception innerException) : base($"Serialization failed for type {type} with exception {innerException.Message}.", innerException)
        {
        }
        public SerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
#if !NET_STANDARD
        public SerializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}
