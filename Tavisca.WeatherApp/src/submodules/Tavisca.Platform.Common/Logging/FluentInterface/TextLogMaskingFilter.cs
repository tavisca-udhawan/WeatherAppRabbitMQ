using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Logging.Fluent
{
    public class TextLogMaskingFilter : ILogFilter
    {
        private readonly Dictionary<string, IMask> _masks = new Dictionary<string, IMask>(StringComparer.OrdinalIgnoreCase);

        public List<TextMaskingRule> Rules { get; } = new List<TextMaskingRule>();

        public TextLogMaskingFilter(TextMaskingRule rule) : this(new TextMaskingRule[] { rule })
        {
        }

        public TextLogMaskingFilter(IEnumerable<TextMaskingRule> rules)
        {
            if (rules != null)
            {
                foreach (var rule in rules)
                {
                    _masks[rule.Field] = rule.Mask;
                    Rules.Add(rule);
                }
            }
        }

        public ILog Apply(ILog log)
        {
            IMask mask = null;
            var items = log.GetFields().ToList();
            var copy = new List<KeyValuePair<string, object>>();
            foreach (var item in items)
            {
                if (_masks.TryGetValue(item.Key, out mask) == false)
                {
                    copy.Add(item);
                }
                else
                {
                    var value = item.Value as string;
                    if (value == null)
                        copy.Add(item);
                    else
                    {
                        try
                        {
                            copy.Add(new KeyValuePair<string, object>(item.Key, mask.Mask(item.Value.ToString())));
                        }
                        catch
                        {
                            copy.Add(new KeyValuePair<string, object>(item.Key, KeyStore.Masking.MaskingFailed));
                            copy.Add(new KeyValuePair<string, object>(KeyStore.Masking.MaskingFailedKey, true));
                        }
                    }
                }
            }
            return new SimpleLog(log.Id, log.LogTime, copy, log.Filters);
        }
    }
}
