
using System;
using System.Net;
using System.Collections.Generic;
using Tavisca.Platform.Common.Models;
namespace Tavisca.Common.Plugins.Configuration
{
    public static partial class Errors
    {
        public static partial class ClientSide
        {

            public static BaseApplicationException InvalidValue(string value)
            {
                return new ConfigurationException(FaultCodes.InvalidValue, string.Format(FaultMessages.InvalidValue, value), HttpStatusCode.BadRequest);
            }

            public static BaseApplicationException NoConfigurationProviderOrSerializer()
            {
                return new ConfigurationException(FaultCodes.NoConfigurationProviderOrSerializer, FaultMessages.NoConfigurationProviderOrSerializer, HttpStatusCode.BadRequest);
            }

        }
    }
}