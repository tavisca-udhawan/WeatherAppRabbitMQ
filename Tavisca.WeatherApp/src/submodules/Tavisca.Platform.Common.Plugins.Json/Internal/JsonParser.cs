using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tavisca.Platform.Common.Plugins.Json
{
    public class JsonParser<T>
    {
        internal JsonParser(Func<T> createNew, T defaultValue, Dictionary<string, Action<T, JValue>> steps)
        {
            _createNew = createNew;
            _defaultValue = defaultValue;
            _steps = steps;
        }

        private readonly Func<T> _createNew;
        private readonly T _defaultValue;
        private readonly Dictionary<string, Action<T, JValue>> _steps;


        public T Parse(JsonSerializer serializer, JsonReader reader)
        {
            T obj = default(T);
            Action<T, JValue> step = null;

            var wrapper = new JValue(reader, serializer);
            string propertyName = string.Empty;

            if (reader.TokenType == JsonToken.Null)
                return _defaultValue;
            while (reader.Read())
            {
                if( reader.TokenType == JsonToken.EndObject )
                    break;
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    if (obj == null) obj = _createNew();
                    propertyName =reader.Value.ToString();
                    if (_steps.TryGetValue(propertyName, out step) == true)
                        step.Invoke(obj, wrapper);
                    else
                        IgnoreProperty(reader);
                    
                    
                }
            }
            return obj;
        }

        private void IgnoreProperty(JsonReader reader)
        {
            JToken jsonObj;
            reader.Read();
            if (reader.TokenType == JsonToken.StartObject || reader.TokenType == JsonToken.StartArray)
                jsonObj = JToken.ReadFrom(reader);
        }
    }
}