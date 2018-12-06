using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Cache
{
    public class CacheHit<T>
    {
        public CacheHit(string key, T value)
        {
            this.Key = key;
            this.Value = value;
        }

        public string Key { get; private set; }

        public T Value { get; private set; }
    }
}
