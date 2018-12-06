
using System;
using System.Net;
using System.Collections.Generic;
using Tavisca.Platform.Common.Models;
namespace Tavisca.Platform.Common.Models
{
    public static partial class Errors
    {
        public static partial class ClientSide
        {

            public static BaseApplicationException RequestNotFound()
            {
                return new BadRequestException(FaultCodes.RequestNotFound, FaultMessages.RequestNotFound, HttpStatusCode.BadRequest);
            }

            public static BaseApplicationException ValidationFailure()
            {
                return new BadRequestException(FaultCodes.ValidationFailure, FaultMessages.ValidationFailure, HttpStatusCode.BadRequest);
            }

            public static BaseApplicationException MandatoryFieldMissing(string fieldName)
            {
                return new BadRequestException(FaultCodes.MandatoryFieldMissing, string.Format(FaultMessages.MandatoryFieldMissing, fieldName), HttpStatusCode.BadRequest);
            }

            public static BaseApplicationException ValueCannotBeNullOrEmptyForMandatoryField(string field)
            {
                return new BadRequestException(FaultCodes.ValueCannotBeNullOrEmptyForMandatoryField, string.Format(FaultMessages.ValueCannotBeNullOrEmptyForMandatoryField, field), HttpStatusCode.BadRequest);
            }

            public static BaseApplicationException InvalidValueForInputType(string path, string property, string type)
            {
                return new BadRequestException(FaultCodes.InvalidValueForInputType, string.Format(FaultMessages.InvalidValueForInputType, path, property, type), HttpStatusCode.BadRequest);
            }

            public static BaseApplicationException InvalidDateTimeFormat(string path, string property)
            {
                return new BadRequestException(FaultCodes.InvalidDateTimeFormat, string.Format(FaultMessages.InvalidDateTimeFormat, path, property), HttpStatusCode.BadRequest);
            }

            public static BaseApplicationException MissingRequestHeaders(List<string> headerNames)
            {
                return new BadRequestException(FaultCodes.MissingRequestHeaders, string.Format(FaultMessages.MissingRequestHeaders, string.Join(", ", headerNames)), HttpStatusCode.BadRequest);
            }

            public static BaseApplicationException InvalidRequestHeaders(List<string> headerNames)
            {
                return new BadRequestException(FaultCodes.InvalidRequestHeaders, string.Format(FaultMessages.InvalidRequestHeaders, string.Join(", ", headerNames)), HttpStatusCode.BadRequest);
            }

            public static BaseApplicationException MissingKeysInParameterStore(List<string> keys)
            {
                return new ConfigurationException(FaultCodes.MissingKeysInParameterStore, string.Format(FaultMessages.MissingKeysInParameterStore, string.Join(", ", keys)), HttpStatusCode.BadRequest);
            }

        }
    }
}