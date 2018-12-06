
using System;
using System.Net;
using System.Collections.Generic;
using Tavisca.Platform.Common.Models;
namespace Tavisca.Platform.Common.Models
{
    public static partial class Errors
    {
        public static partial class ServerSide
        {

            public static BaseApplicationException UnexpectedSystemException()
            {
                return new UnExpectedSystemException(FaultCodes.UnexpectedSystemException, FaultMessages.UnexpectedSystemException, HttpStatusCode.InternalServerError);
            }

            public static BaseApplicationException ServiceCommunication(string serviceName)
            {
                return new CommunicationException(FaultCodes.ServiceCommunication, string.Format(FaultMessages.ServiceCommunication, serviceName), HttpStatusCode.InternalServerError);
            }

            public static BaseApplicationException ParameterStoreCommunicationError()
            {
                return new CommunicationException(FaultCodes.ParameterStoreCommunicationError, FaultMessages.ParameterStoreCommunicationError, HttpStatusCode.InternalServerError);
            }
        }
    }
}