using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Reflection;

namespace Tavisca.Platform.Common.Logging
{
    [Serializable]
    public abstract class LogBase : ILog
    {

        protected LogBase()
        {
            Id = Guid.NewGuid().ToString();
            LogTime = DateTime.UtcNow;
            ProcessId = Process.GetCurrentProcess().Id;
            MachineName = Environment.MachineName;
#if !NET_STANDARD
            AppDomain = System.AppDomain.CurrentDomain.FriendlyName;
#endif
        }

        public virtual IEnumerable<KeyValuePair<string, object>> GetFields()
        {
            // Add core fields
            var map = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                { "id", Id },
                { "log_time", LogTime },
                { "pid", ProcessId },
                { "machine", MachineName },
                { "app_domain", AppDomain },
                { "type", Type },
                { "msg", Message },
                { "app_name", ApplicationName },
                { "tid", TenantId },
                { "cid", CorrelationId },
                { "sid", StackId },
                { "app_txid", ApplicationTransactionId }
            };
            // Add other fields
            var fields = GetLogFields();
            fields?.ForEach(f => map[f.Key] = f.Value);

            // Add attributes
            foreach (var key in _attributes.Keys)
            {
                if (map.ContainsKey(key) == true && map[key] != null)
                    map["attr_" + key] = _attributes[key];
                else
                    map[key] = _attributes[key];
            }
            return map.ToList();
        }

        public List<ILogFilter> Filters { get; } = new List<ILogFilter>();

        protected abstract List<KeyValuePair<string, object>> GetLogFields();


        public void EnableSanitization()
        {
            Filters.Add(new PaymentDataFilter());
        }

        public abstract string Type { get; }

        public string Message { get; set; }

        public string Id { get; set; }

        public string AppDomain { get; set; }

        public DateTime LogTime { get; set; }

        public int ProcessId { get; set; }

        public string MachineName { get; set; }

        public string ApplicationName { get; set; }

        public string TenantId { get; set; }

        public string CorrelationId { get; set; }

        public string StackId { get; set; }

        public string ApplicationTransactionId { get; set; }

        private readonly IDictionary<string, object> _attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        private static readonly IEnumerable<Type> SupportedDataTypes = new List<Type>
        {
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(string),
            typeof(DateTime),
            typeof(bool),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(GeoPoint),
            typeof(Map),
            typeof(Payload),
            typeof(IPAddress)
        };

        private bool TryGetCompatibleType(Type type, out Type compatibleType)
        {
            compatibleType = SupportedDataTypes.FirstOrDefault(t => t.IsAssignableFrom(type) == true);
            return compatibleType != null;
        }

        public bool TrySetValue(string name, object value)
        {
            var type = value?.GetType();
            Type compatibleType = null;
            var isSupported = TryGetCompatibleType(type, out compatibleType);
            if (isSupported == false)
                return false;
            _attributes[name] = value;
            return true;
        }

        public void SetValue(string name, string value)
        {
            _attributes[name] = value;
        }

        public void SetValue(string name, GeoPoint value)
        {
            _attributes[name] = value;
        }

        public void SetValue(string name, DateTime value)
        {
            _attributes[name] = value;
        }

        public void SetValue(string name, long value)
        {
            _attributes[name] = value;
        }

        public void SetValue(string name, int value)
        {
            _attributes[name] = value;
        }

        public void SetValue(string name, ulong value)
        {
            _attributes[name] = value;
        }

        public void SetValue(string name, uint value)
        {
            _attributes[name] = value;
        }

        public void SetValue(string name, float value)
        {
            _attributes[name] = value;
        }

        public void SetValue(string name, double value)
        {
            _attributes[name] = value;
        }

        public void SetValue(string name, decimal value)
        {
            _attributes[name] = value;
        }

        public void SetValue(string name, bool value)
        {
            _attributes[name] = value;
        }

        public void SetValue(string name, byte[] value, Encoding encoding = null)
        {
            _attributes[name] = new Payload(value, encoding);
        }

        public void SetValue(string name, Payload payload)
        {
            _attributes[name] = payload;
        }

        public void SetValue(string name, IDictionary<string, string> map, MapFormat format = MapFormat.DelimitedString)
        {
            _attributes[name] = new Map(map, format);
        }

        public void SetValue(string name, IPAddress ip)
        {
            _attributes[name] = ip;
        }
    }

}
