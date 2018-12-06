using System;
using System.Runtime.Serialization;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    [Serializable]
    public class ClientCommunicationException : Exception
    {
        public ClientCommunicationException() : base()
        {
        }
        public ClientCommunicationException(string message) : base(message)
        {
        }
        public ClientCommunicationException(ApiEndPoint endPoint, Exception innerException): base($"Web Call failed for endpoint url  {endPoint.Url} with exception {innerException.Message}.", innerException)
        {
        }
        public ClientCommunicationException(string message, Exception innerException) : base(message, innerException)
        {
        }
#if !NET_STANDARD
        public ClientCommunicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}
