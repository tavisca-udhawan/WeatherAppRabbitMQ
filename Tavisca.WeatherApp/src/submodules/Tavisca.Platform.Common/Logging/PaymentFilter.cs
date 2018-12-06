using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tavisca.Platform.Common.Logging
{
    public class PaymentDataFilter : ILogFilter
    {
        public ILog Apply(ILog log)
        {
            var fields = log.GetFields().ToList();

            var final = new List<KeyValuePair<string, object>>();
            var candidates = new List<KeyValuePair<string, object>>();
            foreach (var field in fields)
            {
                if (RequiresSanitization(field) == true)
                    candidates.Add(field);
                else
                    final.Add(field);
            }

            foreach (var field in candidates)
                final.Add(Sanitize(field));
            return new SimpleLog(log.Id, log.LogTime, final);
        }

        private KeyValuePair<string, object> Sanitize(KeyValuePair<string, object> field)
        {
            var stringValue = field.Value as string;
            if (stringValue != null)
                return SanitizeAsString(field.Key, stringValue);

            var payload = field.Value as Payload;
            if (payload != null)
                return SanitizeAsBytes(field.Key, payload);

            var map = field.Value as Map;
            if (map != null)
                return SanitizeAsMap(field.Key, map);

            return field;
        }

        private KeyValuePair<string, object> SanitizeAsMap(string field, Map dictionary)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var key in dictionary?.Value.Keys)
                map[key] = CreditcardMask.Mask(dictionary?.Value?[key]);
            return new KeyValuePair<string, object>(field, new Map(map, dictionary.Format));
        }

        private KeyValuePair<string, object> SanitizeAsBytes(string key, Payload payload)
        {
            var value = payload.GetString();
            value = CreditcardMask.Mask(value);
            var newPayload = new Payload(value, payload.Encoding);
            return new KeyValuePair<string, object>(key, newPayload);
        }

        private KeyValuePair<string, object> SanitizeAsString(string key, string value)
        {
            return new KeyValuePair<string, object>(key, CreditcardMask.Mask(value));
        }


        private bool RequiresSanitization(KeyValuePair<string, object> field)
        {
            // The only fields that need to be filtered are strings, maps and byte[]
            // Within strings and byte[], we only need to sanitize long values with lengths greater than a single card number.
            var value = field.Value;
            if (value is IDictionary<string, string>)
                return true;

            var bytes = value as Payload;
            if (bytes != null)
                return bytes.Length > 12;

            var stringValue = value as string;
            if (stringValue?.Length > 12)           // Minimum card length is 13
                return true;
            else
                return false;
        }
    }

    public static class CreditcardMask
    {
        const string PATTERN = @"[0-9]{12,19}";

        private static readonly Regex CardNumber = new Regex(PATTERN, RegexOptions.Compiled);

        public static string Mask(string value)
        {
            var replace = CardNumber.Replace(value, new MatchEvaluator(match =>
            {
                var num = match.ToString();
                if (LuhnsCheck(num) == true)
                    return num.Substring(0, 6) + new string('*', num.Length - 10) +
                      num.Substring(num.Length - 4);
                return num;
            }));
            return replace;
        }

        public static bool LuhnsCheck(string value)
        {
            int sumOfDigits = value.Where(e => e >= '0' && e <= '9')
                            .Reverse()
                            .Select((e, i) => (e - 48) * (i % 2 == 0 ? 1 : 2))
                            .Sum((e) => e / 10 + e % 10);

            return sumOfDigits % 10 == 0;
        }
    }

}
