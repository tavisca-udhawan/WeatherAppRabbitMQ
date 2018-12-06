using System;
using Newtonsoft.Json;

namespace Tavisca.Platform.Common.Plugins.Json
{
    public static class FaultShieldingExtension
    {
        public static JsonConverter WithFaultShielding(this JsonConverter converter, Func<Exception, Exception> onError)
        {
            return new ExceptionShieldingConverter(converter, onError);
        }

        private class ExceptionShieldingConverter : JsonConverter
        {
            private readonly JsonConverter _inner;
            private readonly Func<Exception, Exception> _onError;

            public ExceptionShieldingConverter(JsonConverter converter, Func<Exception, Exception> onError)
            {
                _inner = converter;
                _onError = onError;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                _inner.WriteJson(writer, value, serializer);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                try
                {
                    return _inner.ReadJson(reader, objectType, existingValue, serializer);
                }
                catch (Exception ex)
                {
                    if (_onError != null)
                        throw _onError(ex);
                    else throw;
                }
            }

            public override bool CanConvert(Type objectType)
            {
                return _inner.CanConvert(objectType);
            }
        }
    }
}