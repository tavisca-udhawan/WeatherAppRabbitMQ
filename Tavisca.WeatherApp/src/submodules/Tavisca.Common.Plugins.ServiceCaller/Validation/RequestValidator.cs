using System;

namespace Tavisca.Common.Plugins.ServiceCaller
{

    public class RequestValidator : AbstractValidator<IRequest>
    {
        public override bool Validate(IRequest input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (input.EndPoint == null)
                throw new ArgumentNullException(nameof(input.EndPoint));
            new EndPointValidator().Validate(input.EndPoint);
            if (input.ClientSetting != null)
                new ClientSettingsValidator().Validate(input.ClientSetting);
            return true;
        }
    }
}
