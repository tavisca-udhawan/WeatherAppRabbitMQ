using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace Tavisca.Platform.Common.Plugins.Json
{
    public static class JsonExtensions
    {
        public static JsonWriter WriteField(this JsonWriter writer, string property, string value)
        {
            if (value != null)
            {
                writer.WritePropertyName(property);
                writer.WriteValue(value);
            }
            return writer;
        }

        public static JsonWriter WriteJson(this JsonWriter writer, string property, string value)
        {
            if (value != null)
            {
                writer.WritePropertyName(property);
                writer.WriteRawValue(value);
            }
            return writer;
        }

        public static JsonWriter WriteField(this JsonWriter writer, string property, int value)
        {
            writer.WritePropertyName(property);
            writer.WriteValue(value);
            return writer;
        }

        public static JsonWriter WriteField(this JsonWriter writer, string property, int? value)
        {
            if (value.HasValue)
            {
                writer.WritePropertyName(property);
                writer.WriteValue(value);
            }
            return writer;
        }

        public static JsonWriter WriteField(this JsonWriter writer, string property, long value)
        {
            writer.WritePropertyName(property);
            writer.WriteValue(value);
            return writer;
        }

        public static JsonWriter WriteField(this JsonWriter writer, string property, double value)
        {
            writer.WritePropertyName(property);
            writer.WriteValue(value);
            return writer;
        }

        public static JsonWriter WriteField(this JsonWriter writer, string property, decimal value)
        {
            writer.WritePropertyName(property);
            writer.WriteValue(value);
            return writer;
        }

        public static JsonWriter WriteField(this JsonWriter writer, string property, decimal? value)
        {
            if (value.HasValue)
            {
                writer.WritePropertyName(property);
                writer.WriteValue(value);
            }
            return writer;
        }

        public static JsonWriter WriteField(this JsonWriter writer, string property, DateTime value)
        {
            writer.WritePropertyName(property);
            writer.WriteValue(value);
            return writer;
        }

        public static JsonWriter WriteField(this JsonWriter writer, string property, float value)
        {
            writer.WritePropertyName(property);
            writer.WriteValue(value);
            return writer;
        }

        public static JsonWriter WriteField(this JsonWriter writer, string property, float? value)
        {
            if (value.HasValue)
            {
                writer.WritePropertyName(property);
                writer.WriteValue(value);
            }
            return writer;
        }

        public static JsonWriter WriteField(this JsonWriter writer, string property, bool value)
        {
            writer.WritePropertyName(property);
            writer.WriteValue(value);
            return writer;
        }

        public static JsonWriter WriteField(this JsonWriter writer, string property, bool? value)
        {
            if (value.HasValue)
            {
                writer.WritePropertyName(property);
                writer.WriteValue(value);
            }
            return writer;
        }

        public static JsonWriter WriteField<T>(this JsonWriter writer, string property, T value, JsonSerializer serializer)
        {
            if (value != null)
            {
                writer.WritePropertyName(property);
                serializer.Serialize(writer, value);
            }
            return writer;
        }
        public static JsonWriter WriteArrayField<T>(this JsonWriter writer, string property, List<T> value, JsonSerializer serializer)
        {
            if (value != null && value.Count != 0)
            {
                writer.WritePropertyName(property);
                serializer.Serialize(writer, value);
            }
            return writer;
        }

        public static string ReadAsString(this JObject json, string property, string defaultValue)
        {
            JToken value;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false)
                return defaultValue;
            return value.Value<string>();
        }

        public static string ReadAsString(this JObject json, string property)
        {
            JToken value;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false)
                throw new ArgumentException($"Mandatory field {property} not found.");
            return value.Value<string>();
        }

        public static T ReadAsObject<T>(this JObject json, string property, JsonSerializer serializer)
        {
            JToken value;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false)
                return default(T);
            return serializer.Deserialize<T>(value.CreateReader());
        }

        public static T ReadAsObject<T>(this JObject json, string property, JsonSerializer serializer, T defaultValue)
        {
            JToken value;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false)
                return defaultValue;

            return serializer.Deserialize<T>(value.CreateReader());
        }

        public static int ReadAsInt(this JObject json, string property)
        {
            JToken value;
            int parsed;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false ||
                int.TryParse(value.ToString(), out parsed) == false)
                throw new ArgumentException($"Mandatory field {property} not found.");

            return parsed;
        }

        public static DateTime ReadAsDateTime(this JObject json, string property)
        {
            JToken value;
            DateTime parsed;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false ||
                DateTime.TryParse(value.ToString(), out parsed) == false)
                throw new ArgumentException($"Mandatory field {property} not found.");

            return parsed;
        }
        public static DateTime ReadAsDateTime(this JObject json, string property, DateTime defaultValue)
        {
            JToken value;
            DateTime parsed;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false)
                return defaultValue;
            parsed = DateTime.Parse(value.ToString());
            return parsed;
        }

        public static int ReadAsInt(this JObject json, string property, int defaultValue)
        {
            JToken value;
            int parsed;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false ||
                int.TryParse(value.ToString(), out parsed) == false)
                return defaultValue;

            return parsed;
        }

        public static int? ReadAsNullableInt(this JObject json, string property)
        {
            JToken value;
            int parsed;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false || value.Type == JTokenType.Null)
                return null;

            if (int.TryParse(value.ToString(), out parsed) == false)
                throw new ArgumentException($"Invalid value in field {property} .");

            return parsed;
        }

        public static double ReadAsDouble(this JObject json, string property)
        {
            JToken value;
            double parsed;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false || double.TryParse(value.ToString(), out parsed) == false)
                throw new Exception();

            return parsed;
        }

        public static double ReadAsDouble(this JObject json, string property, double @default)
        {
            JToken value;
            double parsed;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false || double.TryParse(value.ToString(), out parsed) == false)
                return @default;
            return parsed;
        }
        public static float ReadAsFloat(this JObject json, string property)
        {
            JToken value;
            float parsed;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false || float.TryParse(value.ToString(), out parsed) == false)
                throw new Exception();

            return parsed;
        }

        public static float? ReadAsNullableFloat(this JObject json, string property)
        {
            JToken value;
            float parsed;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false || float.TryParse(value.ToString(), out parsed) == false)
                return null;
            return parsed;
        }
        public static bool? ReadAsNullableBoolean(this JObject json, string property)
        {
            JToken value;
            bool parsed;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false || Boolean.TryParse(value.ToString(), out parsed) == false)
                return null;
            return parsed;
        }

        public static float ReadAsFloat(this JObject json, string property, float @default)
        {
            JToken value;
            float parsed;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false || float.TryParse(value.ToString(), out parsed) == false)
                return @default;
            return parsed;
        }

        public static decimal ReadAsDecimal(this JObject json, string property)
        {
            JToken value;
            decimal parsed;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false || decimal.TryParse(value.ToString(), out parsed) == false)
                throw new ArgumentException($"Mandatory field {property} not found.");
            return parsed;
        }

        public static decimal ReadAsDecimal(this JObject json, string property, decimal @default)
        {
            JToken value;
            decimal parsed;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false || decimal.TryParse(value.ToString(), out parsed) == false)
                return @default;
            return parsed;
        }
        public static bool ReadAsBoolean(this JObject json, string property)
        {
            JToken value;
            bool parsed;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false || Boolean.TryParse(value.ToString(), out parsed) == false)
                throw new ArgumentException($"Mandatory field {property} not found.");
            return parsed;
        }

        public static bool ReadAsBoolean(this JObject json, string property, bool @default)
        {
            JToken value;
            bool parsed;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false || Boolean.TryParse(value.ToString(), out parsed) == false)
                return @default;
            return parsed;
        }
        public static T ReadAsEnum<T>(this JObject json, string property, T defaultValue)
            where T : struct
        {
            JToken value;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false)
                return defaultValue;
            T parsed;
            return Enum.TryParse(value.ToString(), true, out parsed) ? parsed : defaultValue;
        }

        public static bool TryReadAsEnum<T>(this JObject json, string property, out T enumValue)
            where T : struct
        {
            enumValue = default(T);
            JToken value;
            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false)
                return false;
            return Enum.TryParse(value.ToString(), true, out enumValue);
        }

        public static T[] ReadAsArray<T>(this JObject json, string property, JsonSerializer serializer)
        {
            JToken value;

            if (json.TryGetValue(property, StringComparison.OrdinalIgnoreCase, out value) == false)
                return new T[] { };

            var array = serializer.Deserialize<T[]>(value.CreateReader()) ?? new T[] { };
            return array.Where(x => x != null).ToArray();
        }
    }
}
