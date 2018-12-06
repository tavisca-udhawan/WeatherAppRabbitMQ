using System.Net;
using Tavisca.Platform.Common.Models;

namespace Tavisca.Common.Plugins.TransitCodeGenerator
{
    public class TransitCodeRequestValidator
    {
        public static bool Validate(TransitCodeRequest request)
        {
            var validationResult = true;

            if (string.IsNullOrEmpty(request.ClientInformation.ClientId))
            {
                ThrowException(FaultCodes.MandatoryFieldMissing, string.Format(FaultMessages.MandatoryFieldMissing, "ClientId"));
            }

            if (string.IsNullOrEmpty(request.ClientInformation.ProgramId))
            {
                ThrowException(FaultCodes.MandatoryFieldMissing, string.Format(FaultMessages.MandatoryFieldMissing, "ProgramId"));
            }

            if (string.IsNullOrEmpty(request.ClientInformation.ProgramCode))
            {
                ThrowException(FaultCodes.MandatoryFieldMissing, string.Format(FaultMessages.MandatoryFieldMissing, "ProgramCode"));
            }

            return validationResult;
        }

        private static void ThrowException(string code, string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            throw new BaseApplicationException(code, message, statusCode);
        }
    }
}
