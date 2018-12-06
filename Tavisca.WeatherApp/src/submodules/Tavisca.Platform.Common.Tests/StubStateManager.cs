using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Spooling;

namespace Tavisca.Platform.Common.Tests
{
    internal class StubStateManager : IStateManager
    {
        private static Dictionary<string, object> _values = new Dictionary<string, object>();

        public Task<KeyValuePair<string, T>[]> MultiGetAsync<T>(string[] keys)
        {
            lock (_values)
            {
                var values = Array.ConvertAll(keys, x => new KeyValuePair<string, T>(x, Get<T>(x)));
                return Task.FromResult(values);
            }
        }

        public Task SetAsync<T>(string key, T value)
        {
            lock (_values)
            {
                _values[key] = value;
            }
            return Task.FromResult(true);
        }

        private T Get<T>(string key)
        {
            object value;
            if (_values.TryGetValue(key, out value) == false)
                return default(T);
            else
                return (T)value;
        }
    }
}