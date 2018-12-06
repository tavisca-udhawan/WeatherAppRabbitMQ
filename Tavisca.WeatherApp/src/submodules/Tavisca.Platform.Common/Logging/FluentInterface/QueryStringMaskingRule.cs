using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Logging.Fluent
{
    public class QueryStringMaskingRule : TextMaskingRule
    {
        public List<TextMaskingRule> FieldMaskingRules { get; } = new List<TextMaskingRule>();

        public QueryStringMaskingRule(string field, params TextMaskingRule[] rules)
        {
            Field = field;
            FieldMaskingRules.AddRange(rules);
            Mask = new FuncMask(new Func<string, string>(target =>
            {
                var keyMap = new Dictionary<string, string>();
                var copy = new Dictionary<string, string>();
                var startIndex = target.IndexOf("?") + 1;
                var payload = startIndex >= 1 ? target.Substring(startIndex) : target;

                foreach (var param in payload?.Split('&')?.Select(x => x.Split('=')))
                {
                    keyMap[param[0]] = param[1];
                };

                foreach (KeyValuePair<string, string> kvp in keyMap)
                {
                    var rule = FindMaskingRule(kvp.Key);
                    if (rule != null)
                    {
                        var value = keyMap[kvp.Key].Trim();
                        copy[kvp.Key] = rule != null && rule.Mask != null ? rule.Mask.Mask(value) : Masks.DefaultMask.Mask(value);
                    }
                    else
                    {
                        copy[kvp.Key] = keyMap[kvp.Key];
                    }
                }

                return (startIndex >= 1 ? target.Substring(0, startIndex) : string.Empty) + String.Join("&", copy.Select(x => string.Format("{0}={1}", x.Key, x.Value)).ToArray());
            }));
        }

        private TextMaskingRule FindMaskingRule(string path)
        {
            return FieldMaskingRules
                .Find(x => new Regex("^" + x.Field + "$", RegexOptions.IgnoreCase).IsMatch(path));
        }
    }
}
