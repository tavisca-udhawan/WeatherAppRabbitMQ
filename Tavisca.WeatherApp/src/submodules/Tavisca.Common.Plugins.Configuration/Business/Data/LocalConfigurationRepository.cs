using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Configuration
{
    public class LocalConfigurationRepository
    {
        private static ConcurrentDictionary<string, string> _kvStorage = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static void Update(string key, string value)
        {

            if (string.IsNullOrWhiteSpace(key))
                return;

            _kvStorage[key] = value;
        }

        public static bool IsKeyPresent(string key)
        {
            return _kvStorage.Keys.Contains(key);
        }

        public static string Get(string key)
        {
            string value = string.Empty;
            var result = _kvStorage.TryGetValue(key, out value) ? value : string.Empty;
            return result;
        }

        public static void ReplaceAll(ConcurrentDictionary<string, string> dataSet)
        {
            _kvStorage = dataSet;
        }

        public static Dictionary<string, string> GetContent()
        {

            return new Dictionary<string, string>(_kvStorage);
        }

    }
}
