using System;
using System.Collections.Generic;

namespace Tavisca.Platform.Common.Plugins.Json
{
    public interface IParserSetup<T>
    {
        IParserSetup<T> Setup(string field, Action<T, JValue> then);
    }

    public class JsonParserBuilder<T> : IParserSetup<T>
    {
        public JsonParserBuilder(Func<T> constructor, T defaultValue)
        {
            _constructor = constructor;
            _defaultValue = defaultValue;
        }


        private readonly Dictionary<string, Action<T, JValue>> _steps = new Dictionary<string, Action<T, JValue>>(StringComparer.OrdinalIgnoreCase);
        private readonly Func<T> _constructor;
        private readonly T _defaultValue;

        public IParserSetup<T> Setup(string field, Action<T, JValue> then)
        {
            _steps[field] = then;
            return this;
        }

        public JsonParser<T> Create()
        {
            return new JsonParser<T>(_constructor, _defaultValue, _steps);
        }
    }
}