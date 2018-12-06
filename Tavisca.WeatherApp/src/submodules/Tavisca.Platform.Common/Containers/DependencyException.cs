using System;
using System.Runtime.Serialization;

namespace Tavisca.Platform.Common.Containers
{
    [Serializable]
    public class DependencyException: Exception
    {
        public DependencyException()
        {
        }

        public DependencyException(string message) : base(message)
        {
        }

        public DependencyException(string message, Exception innerException) : base(message, innerException)
        {
        }
#if !NET_STANDARD
        public DependencyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
#endif
    }
}