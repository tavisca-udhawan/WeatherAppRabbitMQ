using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Spooling
{
    public static class SignatureGenerator
    {
        public static string GetSignature(string secretKey, string data)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                var bytes = Encoding.ASCII.GetBytes($"{secretKey}|{data}");
                var hash = sha256.ComputeHash(bytes);
                var sign = BitConverter.ToString(hash).Replace("-", "");
                if (sign.Length < 32)
                    sign = sign.PadLeft(32, '0');
                return sign.ToLowerInvariant();
            }
        }
    }
}
