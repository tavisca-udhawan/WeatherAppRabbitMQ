using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Logging.Fluent
{
    public abstract class PayloadMaskingRule
    {
        public string Field { get; set; }

        public List<PayloadFieldMaskingRule> FieldMaskingRules { get; } = new List<PayloadFieldMaskingRule>();

        public abstract Payload Apply(Payload target);
    }
}
