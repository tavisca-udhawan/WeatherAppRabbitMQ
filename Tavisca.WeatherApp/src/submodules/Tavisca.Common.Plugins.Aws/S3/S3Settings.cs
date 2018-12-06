using System;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Aws.S3
{
    public class S3Settings
    {
        public static readonly S3Settings Defaults = new S3Settings
        {
            Region = "us-east-1"
        };

        public string Region { get; set; }

        public string AccessKey { get; set; }

        public string SecretKey { get; set; }

        public string Signature { get; private set; }

        public bool HasKeys => string.IsNullOrWhiteSpace(AccessKey) == false &&
                             string.IsNullOrWhiteSpace(SecretKey) == false;

        public static S3Settings Load(NameValueCollection nvc)
        {
            // Parse
            var settings = new S3Settings
            {
                Region = nvc["region"] ?? string.Empty,
                AccessKey = nvc["access_key"] ?? string.Empty,
                SecretKey = nvc["secret_key"] ?? string.Empty
            };

            if (string.IsNullOrEmpty(settings.Region))
                settings.Region = Defaults.Region;

            // Calculate and assign signature
            var data = Encoding.UTF8.GetBytes($"{settings.Region}-{settings.SecretKey}-{settings.AccessKey}");
            using (var sha = SHA512.Create())
            {
                settings.Signature = Convert.ToBase64String(sha.ComputeHash(data));
            }

            return settings;
        }
    }
}

