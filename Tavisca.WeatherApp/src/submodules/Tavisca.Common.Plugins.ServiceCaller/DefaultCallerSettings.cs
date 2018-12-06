using System;
using System.Collections.Specialized;
using System.Text;
using Newtonsoft.Json;

namespace Tavisca.Common.Plugins.ServiceCaller
{
    internal static class DefaultCallerSettings
    {

        static DefaultCallerSettings()
        {
            DefaultSerializerSettings = new JsonSerializerSettings();
            DefaultClientSetting = GetDefaultClientSetting();
            DefaultSerializer = new JsonSerializer();
            DefaultHttpClient = new HttpClient();
            DefaultErrorPayloadType = typeof(ErrorPayLoad);
            
        }
        public static Type DefaultErrorPayloadType { get; private set; }
        public static HttpClient DefaultHttpClient { get; private set; }
        public static Serializer DefaultSerializer { get; private set; }
        public static ClientSetting DefaultClientSetting { get; private set; }
        public static JsonSerializerSettings DefaultSerializerSettings { get; private set; }

        private static ClientSetting GetDefaultClientSetting()
        {
            return new ClientSetting(new TimeSpan(0, 0, 5, 0), new NameValueCollection(), "application/json", 65556, Encoding.UTF8, HttpCompletionOption.ResponseContentRead);
        }
    }
}
