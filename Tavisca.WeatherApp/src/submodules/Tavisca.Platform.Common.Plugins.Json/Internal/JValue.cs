using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tavisca.Platform.Common.Plugins.Json
{
    public class JValue
    {
        private readonly JsonReader _reader;
        private readonly JsonSerializer _serializer;

        public JValue(JsonReader reader, JsonSerializer serializer)
        {
            _reader = reader;
            _serializer = serializer;
        }

        public T AsObject<T>()
        {
            _reader.Read();
            return _serializer.Deserialize<T>(_reader);
        }

        public DateTime AsDateTime(DateTime defaultValue)
        {
            var value = _reader.ReadAsDateTime();
            return value ?? defaultValue;
        }

        public decimal? AsDecimal()
        {
            return _reader.ReadAsDecimal();
        }

        public decimal AsDecimal(decimal defaultValue)
        {
            var value = _reader.ReadAsDecimal();
            return value ?? defaultValue;
        }

        public bool TryAsDecimal(out decimal value)
        {
            value = 0m;
            var jsonValue = _reader.ReadAsDecimal();
            if (!jsonValue.HasValue)
                return false;
            value = jsonValue.Value;
            return true;
        }

        public double? AsDouble()
        {
            return _reader.ReadAsDouble();
        }

        public double AsDouble(double defaultValue)
        {
            var value = _reader.ReadAsDouble();
            return value ?? defaultValue;
        }

        public bool TryAsDouble(out double value)
        {
            value = 0.0d;
            var jsonValue = _reader.ReadAsDouble();
            if (!jsonValue.HasValue)
                return false;
            value = jsonValue.Value;
            return true;
        }

        public float? AsFloat()
        {
            return (float?)_reader.ReadAsDouble();
        }

        public float AsFloat(float defaultValue)
        {
            var value = _reader.ReadAsDouble();
            if (!value.HasValue)
                return defaultValue;
            return (float)value.Value;
        }

        public bool TryAsFloat(out float value)
        {
            value = 0.0f;
            var jsonValue = _reader.ReadAsDouble();
            if (!jsonValue.HasValue)
                return false;
            value = (float)jsonValue.Value;
            return true;
        }

        public int? AsInt()
        {
            return _reader.ReadAsInt32();
        }

        public int AsInt(int defaultValue)
        {
            var value = _reader.ReadAsInt32();
            return value ?? defaultValue;
        }

        public bool TryAsInt(out int value)
        {
            value = 0;
            var jsonValue = _reader.ReadAsInt32();
            if (!jsonValue.HasValue)
                return false;
            value = jsonValue.Value;
            return true;
        }

        public bool? AsBool()
        {
            return _reader.ReadAsBoolean();
        }

        public bool AsBool(bool defaultValue)
        {
            var value = _reader.ReadAsBoolean();
            return value ?? defaultValue;
        }

        public bool TryAsBool(out bool value)
        {
            value = false;
            var jsonValue = _reader.ReadAsBoolean();
            if (!jsonValue.HasValue)
                return false;
            value = jsonValue.Value;
            return true;
        }

        public T[] AsArray<T>()
        {
            while (_reader.TokenType != JsonToken.StartArray && _reader.TokenType != JsonToken.Null)
            {
                _reader.Read();
            }
            var array = _serializer.Deserialize<T[]>(_reader) ?? new T[] { };
            return array.Where(x => x != null).ToArray();
        }


        public string AsString()
        {
            return _reader.ReadAsString();
        }

        public string AsString(string defaultValue)
        {
            return _reader.ReadAsString() ?? defaultValue;
        }

        public T AsEnum<T>(T defaultValue)
            where T : struct
        {
            var value = _reader.ReadAsString();
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;
            T parsed;
            return Enum.TryParse(value, true , out parsed) ? parsed : defaultValue;
        }

        public bool TryParseEnum<T>(out T enumValue)
            where T : struct
        {
            enumValue = default(T);
            var value = _reader.ReadAsString();
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return Enum.TryParse(value, true, out enumValue);
        }

        public JArray GetJsonArray()
        {
            _reader.Read();
            return JArray.Load(_reader);
        }
    }
}