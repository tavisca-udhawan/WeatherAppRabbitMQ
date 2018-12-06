using System;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;

namespace Tavisca.Common.Plugins.Aws
{
    internal class FirehoseSettings
    {
        public static readonly FirehoseSettings Defaults = new FirehoseSettings
        {
            Region = "us-east-1",
            Stream = "logging"
        };

        public string Region { get; set; }

        public string Stream { get; set; }

        public string AccessKey { get; set; }

        public string SecretKey { get; set; }

        public bool HasKeys => string.IsNullOrWhiteSpace(AccessKey) == false && 
                               string.IsNullOrWhiteSpace(SecretKey) == false;

        public string Signature { get; private set; }

        public string RoleArn { get; set; }

        public bool HasRole => string.IsNullOrWhiteSpace(RoleArn) == false;

        public int StsTokenAgeInMinutes { get; set; }

        internal static FirehoseSettings Load(NameValueCollection nvc)
        {   
            var settings = new FirehoseSettings
            {
                Region = nvc["region"] ?? string.Empty,
                AccessKey = nvc["access_key"] ?? string.Empty,
                SecretKey = nvc["secret_key"] ?? string.Empty,
                Stream = nvc["stream"] ?? string.Empty,
                RoleArn = nvc["role_arn"] ?? string.Empty,
                StsTokenAgeInMinutes = GetStsTokenMaxAgeInMins(nvc["sts_token_maxage_minutes"])
            };
            // Parse

            // Handle defaults
            if (string.IsNullOrWhiteSpace(settings.Region) == true)
                settings.Region = Defaults.Region;
            if (string.IsNullOrWhiteSpace(settings.Stream) == true)
                settings.Stream = Defaults.Stream;

            // Calculate and assign signature
            var data = Encoding.UTF8.GetBytes($"{settings.Region}-{settings.SecretKey}-{settings.AccessKey}-{settings.RoleArn}");
            using (var sha = SHA512.Create())
            {
                settings.Signature = Convert.ToBase64String(sha.ComputeHash(data));
            }
            return settings;
        }

        private static int GetStsTokenMaxAgeInMins(string stsTokenAge)
        {
            var defaultStsTokenAge = 60;
            int stsTokenAgeInMinutes;
            if(int.TryParse(stsTokenAge, out stsTokenAgeInMinutes))
            {
                //Valid duration for sts token is 15 Mins to 12 Hours. If value violate that range, default value will be used
                if (stsTokenAgeInMinutes >= 15 && stsTokenAgeInMinutes <= 720)
                    return stsTokenAgeInMinutes;
            }

            return defaultStsTokenAge;
        }
    }
}