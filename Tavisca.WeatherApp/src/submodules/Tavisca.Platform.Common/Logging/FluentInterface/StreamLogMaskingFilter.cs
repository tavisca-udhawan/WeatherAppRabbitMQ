using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Logging;

namespace Tavisca.Platform.Common.Logging.Fluent
{
    public class StreamLogMaskingFilter : ILogFilter
    {
        private readonly Dictionary<string, PayloadMaskingRule> _masks = new Dictionary<string, PayloadMaskingRule>(StringComparer.OrdinalIgnoreCase);

        public IEnumerable<PayloadMaskingRule> Rules
        {
            get
            {
                return _masks.Values;
            }
        }

        public StreamLogMaskingFilter(IEnumerable<PayloadMaskingRule> rules)
        {
            foreach (var rule in rules)
                _masks[rule.Field] = rule;
        }

        public ILog Apply(ILog log)
        {
            PayloadMaskingRule rule = null;
            var items = log.GetFields().ToList();
            var copy = new List<KeyValuePair<string, object>>();

            foreach (var item in items)
            {
                if (_masks.TryGetValue(item.Key, out rule) == false)
                {
                    copy.Add(item);
                }
                else
                {
                    var payload = item.Value as Payload;
                    if (payload == null)
                    {
                        copy.Add(item);
                    }
                    else
                    {
                        try
                        {
                            copy.Add(new KeyValuePair<string, object>(item.Key, rule.Apply(payload)));
                        }
                        catch
                        {
                            copy.Add(new KeyValuePair<string, object>(item.Key, KeyStore.Masking.MaskingFailed));
                            copy.Add(new KeyValuePair<string, object>(KeyStore.Masking.MaskingFailedKey, true));
                        }
                    }
                }
            }
            return new SimpleLog(log.Id, log.LogTime, copy);
        }
    }



    public static class Masks
    {
        /// <summary>
        /// Apply the default masking logic on given input string and return the masked string. Default masking logic depends on the length of input string.
        /// If length of input string is 1-2, the entire string is masked. If length of input string is 3-4, the first character is left as it is and the 
        /// remaining string is masked.If length of input string is greater than 4, first and last characters of the string are left as it is and 
        /// the remaining string is masked. For a string containing space character, each word is masked seperately using the default logic. See examples below
        /// 
        /// INPUT     | OUTPUT
        /// aa        | **
        /// aaaa      | a***
        /// aaaaa     | a***a
        /// aa aaa    | ** a**
        /// aa aaaaa  | ** a***a
        /// </summary>
        /// <param name="input">string to be masked</param>
        /// <returns>masked string</returns>
        public static IMask DefaultMask = new FuncMask(new Func<string, string>(input =>
        {
            string output;

            if (!String.IsNullOrEmpty(input))
            {
                var split = input.Split(' ');
                var masked = new List<string>();
                foreach (var s in split)
                {
                    if (!String.IsNullOrEmpty(s))
                    {
                        int length = s.Length;
                        if (length >= 1 && length <= 2)
                        {
                            masked.Add("".PadLeft(length, '*'));
                        }
                        else if (length >= 3 && length <= 4)
                        {
                            masked.Add(string.Concat(s.Substring(0, 1), "".PadRight(length - 1, '*')));
                        }
                        else
                        {
                            masked.Add(string.Concat(s.Substring(0, 1), "".PadRight(length - 2, '*'), s.Substring(length - 1, 1)));
                        }
                    }
                }

                output = String.Join(" ", masked.ToArray());
            }
            else
            {
                output = input;
            }

            return output; ;
        }));

        public static IMask CreditCardMask = new FuncMask(CreditcardMask.Mask);

        public static IMask MaskCompleteValue = new FuncMask(new Func<string, string>(input =>
        {
            string output;

            if (!String.IsNullOrEmpty(input))
            {
                var split = input.Split(' ');
                var masked = new List<string>();

                foreach (var s in split)
                {
                    if (!String.IsNullOrEmpty(s))
                    {
                        masked.Add("".PadLeft(s.Length, '*'));
                    }
                }

                output = output = String.Join(" ", masked.ToArray());
            }
            else
                output = input;

            return output;
        }));
    }
}