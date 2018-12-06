using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Crypto
{
    internal class Validators
    {
        public static void ValidateIsNullOrEmpty(string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));
        }

        public static void ValidateIsNullOrEmpty(byte[] input)
        {
            if (input == null || input.Length == 0)
                throw new ArgumentNullException(nameof(input));
        }

        internal static void ValidateEncryptRequest(string application, string tenantId, string keyIdentifier, string plainText)
        {
            ValidateIsNullOrEmpty(application);
            ValidateIsNullOrEmpty(tenantId);
            ValidateIsNullOrEmpty(keyIdentifier);
            ValidateIsNullOrEmpty(plainText);
        }

        internal static void ValidateEncryptRequest(string application, string tenantId, string keyIdentifier, byte[] plainTextBytes)
        {
            ValidateIsNullOrEmpty(application);
            ValidateIsNullOrEmpty(tenantId);
            ValidateIsNullOrEmpty(keyIdentifier);
            ValidateIsNullOrEmpty(plainTextBytes);
        }

        internal static void ValidateDecryptRequest(string application, string tenantId, string keyIdentifier, string cipherText)
        {
            ValidateIsNullOrEmpty(application);
            ValidateIsNullOrEmpty(tenantId);
            ValidateIsNullOrEmpty(keyIdentifier);
            ValidateIsNullOrEmpty(cipherText);
        }

        internal static void ValidateDecryptRequest(string application, string tenantId, string keyIdentifier, byte[] cipherTextBytes)
        {
            ValidateIsNullOrEmpty(application);
            ValidateIsNullOrEmpty(tenantId);
            ValidateIsNullOrEmpty(keyIdentifier);
            ValidateIsNullOrEmpty(cipherTextBytes);
        }
    }
}
