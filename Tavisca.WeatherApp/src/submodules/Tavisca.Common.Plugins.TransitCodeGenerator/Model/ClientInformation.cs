using System;
using System.Collections.Generic;
using System.Text;

namespace Tavisca.Common.Plugins.TransitCodeGenerator
{
    [Serializable]
    public class ClientInformation
    {
        public string ClientId { get; set; }
        public string ProgramId { get; set; }
        public string ProgramCode { get; set; }
        public string ClientUniqueID { get; set; }
    }
}
