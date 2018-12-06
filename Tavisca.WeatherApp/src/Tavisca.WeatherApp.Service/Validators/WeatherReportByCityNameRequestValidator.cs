using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.Platform.Common.Models;
using Tavisca.WeatherApp.Service.Data_Contracts;

namespace Tavisca.WeatherApp.Service.Validators
{
    public class WeatherReportByCityNameRequestValidator : AbstractValidator<CityNameRequest>
    {
        public WeatherReportByCityNameRequestValidator()
        {
            RuleFor(x => x.cityName)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull()
                .WithErrorCode(FaultCodes.MandatoryFieldMissing)
                .WithMessage(ErrorMessages.MandatoryFieldMissing("CityName"))
                .NotEmpty()
                .WithErrorCode(FaultCodes.InvalidValueForInputType)
                .WithMessage(ErrorMessages.InvalidValueForInputType("CityName", "cityName", "string"));
        }
    }
}
