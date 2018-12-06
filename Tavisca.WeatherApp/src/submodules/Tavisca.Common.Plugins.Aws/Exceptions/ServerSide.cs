
using System;
using System.Net;
using System.Collections.Generic;
using Tavisca.Platform.Common.Models;
namespace Tavisca.Common.Plugins.Aws
{
	public static partial class Errors 
	{
		public static partial class ServerSide
		{
			
			public static BaseApplicationException S3CommunicationError()
			{
				return new CommunicationException(  FaultCodes.S3CommunicationError, FaultMessages.S3CommunicationError , HttpStatusCode.InternalServerError );
			}
		
			public static BaseApplicationException KMSCommunicationError()
			{
				return new CommunicationException(  FaultCodes.KMSCommunicationError, FaultMessages.KMSCommunicationError , HttpStatusCode.InternalServerError );
			}
		
			public static BaseApplicationException CryptoKeyNotFound()
			{
				return new SystemException(  FaultCodes.CryptoKeyNotFound, FaultMessages.CryptoKeyNotFound , HttpStatusCode.InternalServerError );
			}
		
			public static BaseApplicationException S3BucketMissingConfiguration()
			{
				return new ConfigurationException(  FaultCodes.S3BucketMissingConfiguration, FaultMessages.S3BucketMissingConfiguration , HttpStatusCode.InternalServerError );
			}

		}
	}
}