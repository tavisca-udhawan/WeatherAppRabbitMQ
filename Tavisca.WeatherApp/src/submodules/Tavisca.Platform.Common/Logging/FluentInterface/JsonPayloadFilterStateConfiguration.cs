using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Logging.Fluent
{
    public class JsonPayloadFilterStateConfiguration
    {
        private string _field;

        private List<PayloadFieldMaskingRule> _rules = new List<PayloadFieldMaskingRule>();

        private FilterStateConfiguration _builder;

        public FilterStateConfiguration Builder
        {
            get
            {
                var rule = new JsonPayloadMaskingRule(_field, _rules.ToArray());
                var filter = new StreamLogMaskingFilter(new PayloadMaskingRule[] { rule });
                _builder._actions.Add(f => f.Filters.Add(filter));
                return _builder;
            }
        }

        public JsonPayloadFilterStateConfiguration(FilterStateConfiguration builder, string field)
        {
            _builder = builder;
            _field = field;
        }

        public JsonPayloadFilterStateConfiguration WithField(string path, IMask mask = null)
        {
            var rule = new PayloadFieldMaskingRule() { Path = path, Mask = mask };
            _rules.Add(rule);
            return this;
        }

        public JsonPayloadFilterStateConfiguration WithFieldAsCreditCard(string path)
        {
            return WithField(path, Masks.CreditCardMask);
        }

    }
}
