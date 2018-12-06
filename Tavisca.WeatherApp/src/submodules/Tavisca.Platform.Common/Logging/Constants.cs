using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Logging
{
    internal static class KeyStore
    {
        public static class Masking
        {
            public static readonly string MaskingFailed = "Masking Failed";
            public static readonly string MaskingFailedKey = "masking_failed";
        }

        public static class ConfigurationKeys
        {
            public static readonly string Logging = "logging";
            public static readonly string IsLoggingDisabled = "is_logging_disabled";
        }
    }
}
