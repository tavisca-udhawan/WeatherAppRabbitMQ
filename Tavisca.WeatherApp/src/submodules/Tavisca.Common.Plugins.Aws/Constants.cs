using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Aws
{
    internal static class Constants
    {
        public static readonly string DataEncryptionSection = "data_encryption";
        public static readonly string DataEncryptionKey = "aws_kms";
        public static readonly string KeySpec = "aws_kms_keySpec";
        public static readonly string S3Settings = "s3_settings";
        public static readonly string EnableSanitization = "is_sanitization_enabled";
        public static readonly string BucketName = "s3_bucket_name";

        public static readonly string LogOnlyPolicy = "logonly";

        public static string KeyId = "alias/{0}";
    }
}
