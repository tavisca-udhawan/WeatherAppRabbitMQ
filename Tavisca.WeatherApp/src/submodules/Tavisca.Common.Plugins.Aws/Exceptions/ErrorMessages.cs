using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using Tavisca.Platform.Common.Models;
namespace Tavisca.Common.Plugins.Aws
{
	public static partial class ErrorMessages
	{
		
		public static string S3CommunicationError()
		{
				return FaultMessages.S3CommunicationError;
		}
		
		public static string KMSCommunicationError()
		{
				return FaultMessages.KMSCommunicationError;
		}
		
		public static string CryptoKeyNotFound()
		{
				return FaultMessages.CryptoKeyNotFound;
		}
		
		public static string S3BucketMissingConfiguration()
		{
				return FaultMessages.S3BucketMissingConfiguration;
		}
			}
}