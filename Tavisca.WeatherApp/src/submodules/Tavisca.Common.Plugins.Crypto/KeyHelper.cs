using System;
using System.Security.Cryptography;
using System.Text;

namespace Tavisca.Common.Plugins.Crypto
{
    internal static class KeyHelper
    {
        public static readonly string DefaultKey = "{0}-{1}-{2}";
        internal static string ConstructKey(string application, string tenantId, string keyIdentifier)
        {
            var key = string.Empty;
            var keyValue = string.Format(DefaultKey, application, tenantId, keyIdentifier);

            var data = Encoding.UTF8.GetBytes(keyValue);
            using (var sha = SHA512.Create())
            {
                key = Convert.ToBase64String(sha.ComputeHash(data));
            }

            return key;
        }
    }
}
