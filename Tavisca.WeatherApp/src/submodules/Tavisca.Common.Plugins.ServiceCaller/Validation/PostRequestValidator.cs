using System;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    public class PostRequestValidator<T> : AbstractValidator<WebPostRequest<T>>
    {
        public override bool Validate(WebPostRequest<T> input)
        {
            new RequestValidator().Validate(input);
            if (input.Request == null)
                throw new ArgumentNullException(nameof(input.Request));
            return true;
        }
    }
}
