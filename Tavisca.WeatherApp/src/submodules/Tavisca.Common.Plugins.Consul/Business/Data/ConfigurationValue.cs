using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Configuration
{
    public class ConfigurationValue : IConfigurationValue
    {
        private readonly string _value;
        private readonly IJsonSerializer _jsonSerializer;
        public ConfigurationValue(string value, IJsonSerializer jsonSerializer)
        {
            _value = value;
            _jsonSerializer = jsonSerializer;
        }

        public T GetAs<T>(T defaultValue = default(T))
        {
            if (string.IsNullOrEmpty(_value))
                return defaultValue;

            if (typeof(T) == typeof(string))
                return (T)Convert.ChangeType(_value, typeof(T));

            try
            {
                return _jsonSerializer.Deserialize<T>(_value);
            }
            catch (JsonSerializationException ex)
            {
                throw new SerializationException(ErrorMessages.ConfigurationValueReadFailed(), FaultCodes.ConfigurationValueReadFailed, ex);
            }
            catch (JsonReaderException ex)
            {
                throw new SerializationException(ErrorMessages.ConfigurationValueReadFailed(), FaultCodes.ConfigurationValueReadFailed, ex);
            }
        }

        public NameValueCollection GetAsNameValueCollcetion(NameValueCollection defaultValue = null)
        {
            if (string.IsNullOrEmpty(_value))
                return defaultValue;

            try
            {
                Dictionary<string, string> kvps = _jsonSerializer.Deserialize<Dictionary<string, string>>(_value);
                var nameValueCollection = ToNameValueCollection(kvps);
                if (nameValueCollection == null)
                    return defaultValue;

                return nameValueCollection;

            }
            catch (JsonReaderException ex)
            {
                throw new SerializationException(ErrorMessages.ConfigurationValueReadFailed(), FaultCodes.ConfigurationValueReadFailed, ex);
            }
        }

        public string GetAsString(string defaultValue = null)
        {
            if (string.IsNullOrEmpty(_value))
                return defaultValue;

            return _value;
        }

        private static NameValueCollection ToNameValueCollection(IDictionary<string, string> dictionary)
        {
            if (dictionary == null || !dictionary.Any())
                return null;
             
            var nameValueCollection = new NameValueCollection();

            foreach (var kvp in dictionary)
            {
                nameValueCollection.Add(kvp.Key, kvp.Value);
            }
            return nameValueCollection;
        }
    }
}
