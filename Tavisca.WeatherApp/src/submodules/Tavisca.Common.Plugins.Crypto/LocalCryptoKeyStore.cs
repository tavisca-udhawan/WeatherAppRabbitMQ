using System;
using System.Collections.Concurrent;
using System.Security;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Crypto
{
    public class LocalCryptoKeyStore
    {
        private static ConcurrentDictionary<string, SecureString> _keyStorage = new ConcurrentDictionary<string, SecureString>(StringComparer.OrdinalIgnoreCase);

        public static void Update(string key, SecureString value)
        {

            if (string.IsNullOrWhiteSpace(key))
                return;

            _keyStorage[key] = value;
        }

        public static SecureString Get(string key)
        {
            SecureString value;
            var result = _keyStorage.TryGetValue(key, out value) ? value : null;
            return result;
        }
    }
}
