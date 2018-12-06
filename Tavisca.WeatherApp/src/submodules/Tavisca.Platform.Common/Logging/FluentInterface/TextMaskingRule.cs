using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Logging.Fluent
{
    public class TextMaskingRule
    {
        public string Field { get; set; }

        public IMask Mask { get; set; }

        public virtual string Apply(string target)
        {
            return Mask.Mask(target);
        }
    }
}
