using System.Collections.Concurrent;

namespace Tavisca.Common.Plugins.WebClient
{
    public static class WebClientFactory
    {
        private static ConcurrentDictionary<Settings, IWebClient> _webClientStore = new ConcurrentDictionary<Settings, IWebClient>();
        private static object _lock = new object();
        
        public static IWebClient Create(Settings settings)
        {

            if(_webClientStore.ContainsKey(settings))
            {
                return _webClientStore[settings];
            }
            lock(_lock)
            {
                if (!_webClientStore.ContainsKey(settings))
                {

                    var webClient = settings.TimeOut.HasValue ? new WebClient(settings.TimeOut.Value) : new WebClient();
                    _webClientStore[settings] = webClient;

                }
                return _webClientStore[settings];
            }
        }
    }
}
