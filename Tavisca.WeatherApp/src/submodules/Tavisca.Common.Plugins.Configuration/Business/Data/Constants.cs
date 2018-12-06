using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Configuration
{
    internal static class Constants
    {
        public static readonly string DefaultConsulConnection = "http://172.16.3.84:9500";
        public static readonly string TenantKey = "tenant/{0}/{1}/{2}/{3}";
        public static readonly string GlobalKey = "{0}/{1}/{2}/{3}";
        public static readonly string DefaultKey = "{0}/{1}/{2}/{3}";
        public static readonly string Global = "global";
        public static readonly string Default = "default";
        public static readonly string HttpString = "http://";
        public static readonly string ConsulConnectionString = "consulConnectionString";
        public static readonly string DefaultPolicy = "default";
        public static readonly string LogOnlyPolicy = "logonly";

        internal static class Section
        {
            public static readonly string FeatureFlagSettings = "feature_flag_settings";

        }
    }
}
