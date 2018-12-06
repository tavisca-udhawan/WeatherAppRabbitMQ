using System;
using Tavisca.Common.Plugins.WebClient;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    public sealed class ErrorPayloadTypeSelector
    {
        public delegate Type GetErrorPayloadType(WebClientResponseMessage webClientResponseMessage, Type selectedType);

        private readonly GetErrorPayloadType _errorPayloadtype;

        public ErrorPayloadTypeSelector(GetErrorPayloadType returnObjectTypeHandler)
        {
            _errorPayloadtype = returnObjectTypeHandler;
        }

        public Type GetReturnObjectType(WebClientResponseMessage webClientResponseMessage, Type selectedType)
        {
            return _errorPayloadtype.Invoke(webClientResponseMessage, selectedType);
        }
    }
}
