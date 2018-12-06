using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.Platform.Common.Models;
using Tavisca.WeatherApp.Service.Data_Contracts;

namespace Tavisca.WeatherApp.Service.Validators
{
    public class WeatherReportByZipCodeRequestValidator:AbstractValidator<ZipCodeRequest>
    {
        public WeatherReportByZipCodeRequestValidator()
        {
            RuleFor(x => x.zipCode)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull()
                .WithErrorCode(FaultCodes.MandatoryFieldMissing)
                .WithMessage(ErrorMessages.MandatoryFieldMissing("ZipCode"))
                .NotEmpty()
                .WithErrorCode(FaultCodes.InvalidValueForInputType)
                .WithMessage(ErrorMessages.InvalidValueForInputType("ZipCode", "zipCode", "string"));
        }
    }
}
