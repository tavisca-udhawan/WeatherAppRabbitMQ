using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.Platform.Common.Models;
using Tavisca.WeatherApp.Service.Data_Contracts;

namespace Tavisca.WeatherApp.Service.Validators
{
    public class WeatherReportByCityIdRequestValidator:AbstractValidator<CityIdRequest>
    {
        public WeatherReportByCityIdRequestValidator()
        {
            RuleFor(x => x.cityId)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotNull()
                .WithErrorCode(FaultCodes.MandatoryFieldMissing)
                .WithMessage(ErrorMessages.MandatoryFieldMissing("CityId"))
                .NotEmpty()
                .WithErrorCode(FaultCodes.InvalidValueForInputType)
                .WithMessage(ErrorMessages.InvalidValueForInputType("CityId", "cityId", "string"));
        }
    }
}
