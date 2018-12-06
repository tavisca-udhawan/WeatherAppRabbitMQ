using System;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;

namespace Tavisca.Common.Plugins.Aws.KMS
{
    public class KmsSettings
    {
        public static readonly KmsSettings Defaults = new KmsSettings
        {
            Region = "us-east-1"
        };

        public string Region { get; set; }

        public string AccessKey { get; set; }

        public string SecretKey { get; set; }

        public string Signature { get; private set; }

        public bool HasKeys => string.IsNullOrWhiteSpace(AccessKey) == false &&
                             string.IsNullOrWhiteSpace(SecretKey) == false;

        public static KmsSettings Load(NameValueCollection nvc)
        {
            // Parse
            var settings = new KmsSettings
            {
                Region = nvc["region"] ?? string.Empty,
                AccessKey = nvc["access_key"] ?? string.Empty,
                SecretKey = nvc["secret_key"] ?? string.Empty,
            };

            // Handle defaults
            if (string.IsNullOrWhiteSpace(settings.Region) == true)
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
