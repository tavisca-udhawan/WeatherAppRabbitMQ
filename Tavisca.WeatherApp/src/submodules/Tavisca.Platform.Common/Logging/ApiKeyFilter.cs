using System;
using System.Collections.Generic;
using System.Linq;

namespace Tavisca.Platform.Common.Logging
{
    public class ApiKeyFilter : ILogFilter
    {
        public ILog Apply(ILog log)
        {
            var fields = log.GetFields().ToList();
            var final = new List<KeyValuePair<string, object>>();
            var candidates = new List<KeyValuePair<string, object>>();
            foreach (var field in fields)
            {
                if (field.Value is Map)
                    candidates.Add(field);
                else
                    final.Add(field);
            }

            foreach (var field in candidates)
                final.Add(Sanitize(field));
            return new SimpleLog(log.Id, log.LogTime, final, log.Filters);
        }

        private KeyValuePair<string, object> Sanitize(KeyValuePair<string, object> field)
        {
            var map = field.Value as Map;
            if (map?.Value?.Count > 0)
            {
                string value;
                if (map.Value.TryGetValue("oski-apikey", out value) == false)
                    return field;
                map.Value["oski-apikey"] = MaskApiKey(value);
            }
            return field;
        }

        private string MaskApiKey(string value)
        {
            /*
             * If length of value is less than 5 then return as is.
             * Else mask first and last 2-4 characters based on length.
             */
            if (value.Length <= 5)
                return value;
            var maskLength = 2 + Math.Min(2, ((value.Length - 4)/2) - 1);
            return value.Substring(0, maskLength) + "******" + value.Substring(value.Length - maskLength);
        }
    }
}
