
using System;
using System.Net;
using System.Collections.Generic;
using Tavisca.Platform.Common.Models;
namespace Tavisca.Common.Plugins.Configuration
{
    public static partial class Errors
    {
        public static partial class ServerSide
        {

            public static BaseApplicationException InvalidDataFromEvents()
            {
                return new ConfigurationException(FaultCodes.InvalidDataFromEvents, FaultMessages.InvalidDataFromEvents, HttpStatusCode.InternalServerError);
            }

            public static BaseApplicationException ConsulConfigurationClientNotAvailable()
            {
                return new ConfigurationException(FaultCodes.ConsulConfigurationClientNotAvailable, FaultMessages.ConsulConfigurationClientNotAvailable, HttpStatusCode.InternalServerError);
            }

            public static BaseApplicationException ConfigurationValueReadFailed()
            {
                return new SerializationException(FaultCodes.ConfigurationValueReadFailed, FaultMessages.ConfigurationValueReadFailed, HttpStatusCode.InternalServerError);
            }

            public static BaseApplicationException FailedToDecodePayloadInConfigurationClient()
            {
                return new ApplicationEventException(FaultCodes.FailedToDecodePayloadInConfigurationClient, FaultMessages.FailedToDecodePayloadInConfigurationClient, HttpStatusCode.InternalServerError);
            }

        }
    }
}