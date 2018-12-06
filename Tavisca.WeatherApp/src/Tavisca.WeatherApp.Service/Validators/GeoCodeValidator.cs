using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.Platform.Common.Models;
using Tavisca.WeatherApp.Model;

namespace Tavisca.WeatherApp.Service.Validators
{
    internal class GeoCodeValidator : AbstractValidator<GeoCode>
    {
        public GeoCodeValidator()
        {
            RuleFor(x => x.Latitude)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull()
                .WithErrorCode(FaultCodes.MandatoryFieldMissing)
                .WithMessage(ErrorMessages.MandatoryFieldMissing("Latitude"))
                .NotEmpty()
                .WithErrorCode(FaultCodes.InvalidValueForInputType)
                .WithMessage(ErrorMessages.InvalidValueForInputType("Latitude", "latitude", "string"))
                .WithMessage(ErrorMessages.InvalidValueForInputType("GeoCode", "latitude", "string"));
            ;
            RuleFor(x => x.Longitude)
              .Cascade(CascadeMode.StopOnFirstFailure)
              .NotNull()
              .WithErrorCode(FaultCodes.MandatoryFieldMissing)
              .WithMessage(ErrorMessages.MandatoryFieldMissing("Longitude"))
               .NotEmpty()
               .WithErrorCode(FaultCodes.InvalidValueForInputType)
               .WithMessage(ErrorMessages.InvalidValueForInputType("Longitude", "longitude", "string"))
                .WithMessage(ErrorMessages.InvalidValueForInputType("GeoCode", "longitude", "string"));
        }
    }
}
