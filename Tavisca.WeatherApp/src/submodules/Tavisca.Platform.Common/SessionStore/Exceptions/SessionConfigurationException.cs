using System;

namespace Tavisca.Common.Plugins.SessionStore.Exceptions
{
    [Serializable]
    public class SessionConfigurationException : SessionException
    {
        public SessionConfigurationException() { }

        public SessionConfigurationException(string message) : base(message) { }

        public SessionConfigurationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
