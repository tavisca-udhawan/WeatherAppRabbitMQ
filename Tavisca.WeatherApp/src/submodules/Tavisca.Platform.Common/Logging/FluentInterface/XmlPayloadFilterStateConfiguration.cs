using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Logging.Fluent
{
    public class XmlPayloadFilterStateConfiguration
    {
        private string _field;

        private Dictionary<string, string> _namespaces = new Dictionary<string, string>();

        private List<PayloadFieldMaskingRule> _rules = new List<PayloadFieldMaskingRule>();

        private FilterStateConfiguration _builder;

        public FilterStateConfiguration Builder
        {
            get
            {
                var rule = new XmlPayloadMaskingRule(_field, _namespaces, _rules.ToArray());
                var filter = new StreamLogMaskingFilter(new PayloadMaskingRule[] { rule });
                _builder._actions.Add(f => f.Filters.Add(filter));
                return _builder;
            }
        }

        public XmlPayloadFilterStateConfiguration(FilterStateConfiguration builder, string field, Dictionary<string, string> namespaces = null)
        {
            _builder = builder;
            _field = field;

            if (namespaces != null && namespaces.Count > 0)
            {
                foreach (KeyValuePair<string, string> kvp in namespaces)
                {
                    _namespaces[kvp.Key] = kvp.Value;
                }
            }
        }

        public XmlPayloadFilterStateConfiguration WithField(string path, IMask mask = null)
        {
            var rule = new PayloadFieldMaskingRule() { Path = path, Mask = mask };
            _rules.Add(rule);
            return this;
        }

        public XmlPayloadFilterStateConfiguration WithFieldAsCreditCard(string path)
        {
            return WithField(path, Masks.CreditCardMask);
        }

    }
}
