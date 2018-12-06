using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Logging.Fluent
{
    public class FilterStateConfiguration
    {
        internal ILoggingHttpFilter _filter;

        internal List<Action<ILoggingHttpFilter>> _actions = new List<Action<ILoggingHttpFilter>>();

        public FilterStateConfiguration(ILoggingHttpFilter filter)
        {
            _filter = filter;
        }

        public FilterStateConfiguration MaskField(string field, IMask mask = null)
        {
            var filter = new TextLogMaskingFilter(new TextMaskingRule() { Field = field, Mask = mask ?? Masks.DefaultMask });
            _actions.Add(f => f.Filters.Add(filter));
            return this;
        }

        public FilterStateConfiguration MaskFieldAsCreditCard(string field)
        {
            return MaskField(field, Masks.CreditCardMask);
        }

        public JsonPayloadFilterStateConfiguration MaskPayloadAsJson(string field)
        {
            return new JsonPayloadFilterStateConfiguration(this, field);
        }

        public XmlPayloadFilterStateConfiguration MaskPayloadAsXml(string field, Dictionary<string, string> namespaces = null)
        {
            return new XmlPayloadFilterStateConfiguration(this, field, namespaces);
        }


        public QueryStringFilterStateConfiguration MaskPayloadAsQueryString(string field)
        {
            return new QueryStringFilterStateConfiguration(this, field);
        }

        public ILoggingHttpFilter Apply()
        {
            _actions.ForEach(x => x(_filter));
            return _filter;
        }
    }
}
