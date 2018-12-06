using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Logging.Fluent
{
    public class QueryStringFilterStateConfiguration
    {
        private string _field;

        private List<TextMaskingRule> _rules = new List<TextMaskingRule>();

        private FilterStateConfiguration _builder;

        public FilterStateConfiguration Builder
        {
            get
            {
                var rule = new QueryStringMaskingRule(_field, _rules.ToArray());
                var filter = new TextLogMaskingFilter(new QueryStringMaskingRule[] { rule });
                _builder._actions.Add(f => f.Filters.Add(filter));
                return _builder;
            }
        }

        public QueryStringFilterStateConfiguration(FilterStateConfiguration builder, string field)
        {
            _builder = builder;
            _field = field;
        }

        public QueryStringFilterStateConfiguration WithField(string path, IMask mask = null)
        {
            var rule = new TextMaskingRule() { Field = path, Mask = mask };
            _rules.Add(rule);
            return this;
        }

        public QueryStringFilterStateConfiguration WithFieldAsCreditCard(string path)
        {
            return WithField(path, Masks.CreditCardMask);
        }

    }
}
