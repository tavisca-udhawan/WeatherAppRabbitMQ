using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace Tavisca.Platform.Common.Plugins.Json
{
    internal class MappedTypeContractResolver : DefaultContractResolver
    {
        public MappedTypeContractResolver(ITranslatorMapping mapping) : base()
        {
            mapping?.Configure(_mappings);
        }

        public MappedTypeContractResolver(ITranslatorMapping mapping, List<Tuple<Type, JsonConverter>> customConverters) : base()
        {
            mapping?.Configure(_mappings);
            customConverters.ForEach(c => _mappings[c.Item1] = c.Item2);
        }

        private readonly Dictionary<Type, JsonConverter> _mappings = new Dictionary<Type, JsonConverter>();

        public override JsonContract ResolveContract(Type type)
        {
            JsonConverter converter = null;
            if (_mappings.TryGetValue(type, out converter) == true)
                return new JsonObjectContract(type) { Converter = converter };
            else
                return base.ResolveContract(type);
        }

        protected override string ResolvePropertyName(string propertyName)
        {
            return ToCamelCase(propertyName);
        }

        //Following is picked up as is from json.net camelcase resolver
        private static string ToCamelCase(string s)
        {
            if (string.IsNullOrEmpty(s) || !char.IsUpper(s[0]))
            {
                return s;
            }
            char[] array = s.ToCharArray();
            int num = 0;
            while (num < array.Length && (num != 1 || char.IsUpper(array[num])))
            {
                bool flag = num + 1 < array.Length;
                if ((num > 0 & flag) && !char.IsUpper(array[num + 1]))
                {
                    break;
                }
                array[num] = char.ToLowerInvariant(array[num]);
                num++;
            }
            return new string(array);
        }
    }
}
