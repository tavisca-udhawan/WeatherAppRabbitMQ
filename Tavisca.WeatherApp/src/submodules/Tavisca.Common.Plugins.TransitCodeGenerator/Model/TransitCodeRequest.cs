using System;

namespace Tavisca.Common.Plugins.TransitCodeGenerator
{
    [Serializable]
    public class TransitCodeRequest
    {
        public ClientInformation ClientInformation { get; set; }
        public string Mode { get; set; }
        public string RequestedBy { get; set; }
        public string TransitCode { get; set; }
        public string UserType { get; set; }
        public string Lcid { get; set; }
        public string ClientEnvironmentToken { get; set; }
    }
}
