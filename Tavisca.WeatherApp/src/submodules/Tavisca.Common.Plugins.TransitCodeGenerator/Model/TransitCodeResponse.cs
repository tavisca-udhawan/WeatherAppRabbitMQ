using System;
using System.Collections.Generic;
using System.Text;

namespace Tavisca.Common.Plugins.TransitCodeGenerator
{
    public class TransitCodeResponse
    {
        public string Message { get; set; }
        public string Status { get; set; }
        public string TransitCode { get; set; }
    }
}
