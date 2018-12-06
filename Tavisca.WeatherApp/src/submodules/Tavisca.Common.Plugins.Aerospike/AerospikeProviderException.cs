using System;
using System.Runtime.Serialization;

namespace Tavisca.Common.Plugins.Aerospike
{
    [Serializable]
    public class AerospikeProviderException : Exception
    {
        public AerospikeProviderException(string message) : base(message)
        {

        }

        public AerospikeProviderException(string message, Exception inner) : base(message, inner)
        {

        }

        protected AerospikeProviderException(SerializationInfo info, StreamingContext context)
        {

        }
    }
}
