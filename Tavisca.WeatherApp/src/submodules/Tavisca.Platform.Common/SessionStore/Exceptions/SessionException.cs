using System;

namespace Tavisca.Common.Plugins.SessionStore.Exceptions
{
    [Serializable]
    public class SessionException : Exception
    {
        public SessionException() { }

        public SessionException(string message) : base(message) { }

        public SessionException(string message, Exception innerException) : base(message, innerException) { }
    }
}
