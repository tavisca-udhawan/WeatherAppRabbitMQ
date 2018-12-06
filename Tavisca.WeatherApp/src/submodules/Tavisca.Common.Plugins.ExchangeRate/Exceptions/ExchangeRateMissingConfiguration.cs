using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.ExchangeRate.Exceptions
{
    public class ExchangeRateMissingConfiguration : Exception, ISerializable
    {
        private const string message = "Provider Configuration are missing.";
        public ExchangeRateMissingConfiguration() : base(message)
        {

        }

        public ExchangeRateMissingConfiguration(string property) : base(string.Format("Provider Configuration for '{0}' is missing.", property))
        {            

        }       

        // This constructor is needed for serialization.
        protected ExchangeRateMissingConfiguration(SerializationInfo info, StreamingContext context) : base(info, context)
        {

        }
    }
}
