using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Redis
{
    [Serializable]
    public class RedisProviderException : Exception
    {
        public RedisProviderException(string message) : base(message)
        {

        }

        public RedisProviderException(string message, Exception inner) : base(message, inner)
        {

        }

        protected RedisProviderException(SerializationInfo info, StreamingContext context)
        {

        }
    }
}
