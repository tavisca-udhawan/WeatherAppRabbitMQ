using System;
using Newtonsoft.Json;

namespace Tavisca.Platform.Common.Plugins.Json
{
    public abstract class JsonTranslator<T> : JsonConverter
    {
        private JsonParser<T> _parser = null;

        private JsonParser<T> GetParser()
        {
            if( _parser == null )
                _parser = CreateParser();
            return _parser;
        }

        protected JsonParser<T> CreateParser()
        {
            var parser = new JsonParserBuilder<T>(CreateNew, GetDefaultValue());
            SetupParser(parser);
            return parser.Create();
        }

        protected virtual T GetDefaultValue()
        {
            return default(T);
        }

        protected abstract T CreateNew();

        protected abstract void SetupParser(IParserSetup<T> parser);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return GetParser().Parse(serializer, reader);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            
            if (value == null )
                writer.WriteNull();
            else
                Serialize(writer, (T)value, serializer);
        }

        protected abstract void Serialize(JsonWriter writer, T value, JsonSerializer serializer);

        public override bool CanConvert(Type objectType)
        {
            return typeof(T) == objectType;
        }
    }
}
