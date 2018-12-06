using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tavisca.Platform.Common.Models;
using Tavisca.Platform.Common.Serialization;

namespace Tavisca.Platform.Common.WebApi
{
    public class InfoTranslator: JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var info = value as Info;
            if (info == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();
            writer.WritePropertyName("code");
            writer.WriteValue(info.Code);
            writer.WritePropertyName("message");
            writer.WriteValue(info.Message);
            writer.WriteEndObject();
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof (Info) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var json = JToken.ReadFrom(reader) as JObject;
            if (json == null)
                return null;
            JToken value;
            string code = null, message = null;
            if (json.TryGetValue("code", out value) == true)
                code = value.ToString();
            if (json.TryGetValue("message", out value) == true)
                message = value.ToString();
            return new Info(code, message);
        }
    }
}
