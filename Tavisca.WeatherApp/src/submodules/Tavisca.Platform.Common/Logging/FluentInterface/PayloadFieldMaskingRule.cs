using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Logging.Fluent
{
    public class PayloadFieldMaskingRule
    {
        public string Path { get; set; }

        public IMask Mask { get; set; }
    }
}
