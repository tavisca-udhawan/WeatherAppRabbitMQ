using System;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    public class ClientSettingsValidator : AbstractValidator<ClientSetting>
    {
        public override bool Validate(ClientSetting input)
        {
            if (input != null)
            {
                if (string.IsNullOrEmpty(input.ContentType))
                    throw new ArgumentNullException(nameof(input.ContentType));
                if (input.MaxResponseBufferSize == 0)
                    throw new ArgumentException("MaxResponseBufferSizeValue should not be zero or less than zero",
                        nameof(input.MaxResponseBufferSize));
                if (input.TimeOut == TimeSpan.Zero)
                    throw new ArgumentException("Timeout value should not be zero", nameof(input.TimeOut));
                if (input.Encoding == null)
                    throw new ArgumentNullException(nameof(input.Encoding));
            }
            return true;

        }
    }

    public class EndPointValidator : AbstractValidator<ApiEndPoint>
    {
        public override bool Validate(ApiEndPoint input)
        {
            if (string.IsNullOrEmpty(input.Url))
                throw new ArgumentNullException(nameof(input.Url));
            return true;
        }
    }
}
