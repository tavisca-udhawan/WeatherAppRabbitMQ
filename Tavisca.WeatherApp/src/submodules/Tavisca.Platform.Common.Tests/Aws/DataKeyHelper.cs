using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Tavisca.Platform.Common.Tests.Aws
{
    internal static class DataKeyHelper
    {
        public static string ConvertSecureStrToString(SecureString secureKey)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureKey);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }

        public static string ConvertBytesToString(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }
    }
}
