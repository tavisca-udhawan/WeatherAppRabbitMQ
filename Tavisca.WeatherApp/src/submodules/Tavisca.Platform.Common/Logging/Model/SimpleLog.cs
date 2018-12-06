using System;
using System.Collections.Generic;
using System.Linq;

namespace Tavisca.Platform.Common.Logging
{
    [Serializable]
    public class SimpleLog : ILog
    {
        private readonly Dictionary<string, object> _items = new Dictionary<string, object>(StringComparer.CurrentCultureIgnoreCase);

        public SimpleLog(string id, DateTime logTime, IEnumerable<KeyValuePair<string, object>> items, IEnumerable<ILogFilter> filters = null)
        {
            Id = id;
            LogTime = logTime;
            foreach (var item in items)
                _items[item.Key] = item.Value;
            if (filters != null)
                Filters.AddRange(filters);
        }

        public List<ILogFilter> Filters { get; } = new List<ILogFilter>();

        public string Id { get; set;  }

        public DateTime LogTime { get; set; }

        public IEnumerable<KeyValuePair<string, object>> GetFields()
        {
            return _items.ToArray();
        }
    }
    
}
