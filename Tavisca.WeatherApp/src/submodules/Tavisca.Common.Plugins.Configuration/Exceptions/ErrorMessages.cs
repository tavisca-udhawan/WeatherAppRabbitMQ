using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Tavisca.Platform.Common.Models;
namespace Tavisca.Common.Plugins.Configuration
{
    public static partial class ErrorMessages
    {

        public static string InvalidValue(string value)
        {
            return string.Format(FaultMessages.InvalidValue, value);
        }

        public static string NoConfigurationProviderOrSerializer()
        {
            return FaultMessages.NoConfigurationProviderOrSerializer;
        }

        public static string InvalidDataFromEvents()
        {
            return FaultMessages.InvalidDataFromEvents;
        }

        public static string ConsulConfigurationClientNotAvailable()
        {
            return FaultMessages.ConsulConfigurationClientNotAvailable;
        }

        public static string ConfigurationValueReadFailed()
        {
            return FaultMessages.ConfigurationValueReadFailed;
        }

        public static string FailedToDecodePayloadInConfigurationClient()
        {
            return FaultMessages.FailedToDecodePayloadInConfigurationClient;
        }
    }
}