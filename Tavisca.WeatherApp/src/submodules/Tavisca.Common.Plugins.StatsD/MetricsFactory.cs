using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.Metrics;

namespace Tavisca.Common.Plugins.StatsD
{
    public class MetricsFactory : IMeteringFactory
    {
        public MetricsFactory( IConfigurationProvider configuration, string application, int cacheLifetimeInSeconds = 600)
        {
            _configuration = configuration;
            _application = string.IsNullOrWhiteSpace(application) ? "app" : application;
            _configCache = new InprocCache<NameValueCollection>(new TimeSpan(0, 0, cacheLifetimeInSeconds));
            _meterCache = new InprocCache<IMeter>(new TimeSpan(0, 0, cacheLifetimeInSeconds));
        }


        private readonly IConfigurationProvider _configuration;
        private static IDictionary<string, IMeter> _cache = new Dictionary<string, IMeter>(StringComparer.OrdinalIgnoreCase);
        private readonly string _application;
        private readonly string GlobalKey = "{5B5DAF3F-A77D-4C36-9598-7F8A5107BE41}";
        private InprocCache<IMeter> _meterCache;
        private InprocCache<NameValueCollection> _configCache;
        public static int TimeoutInMs = 1000;

        public IMeter CreateNew(string scope = null)
        {
            try
            {
                if (_configuration == null)
#if DEBUG
                    throw new Exception("Metrics configuration not defined.");
#else
            return NullMeter.Instance;       
#endif
                
                return GetMetric(scope);
            }
            catch
            {
                return NullMeter.Instance;
            }
        }

        private IMeter GetMetric(string scope)
        {
            string key = scope;
            if (string.IsNullOrWhiteSpace(scope) == true)
                key = GlobalKey;

            var settings = _configCache.GetExistingOrCreate(key, () => GetSettings(scope));
            if (settings == null || settings.Count == 0)
                return NullMeter.Instance;
            return CreateMeter(scope, settings);
        }


        private NameValueCollection GetSettings(string scope)
        {
            NameValueCollection settings = null;

            if (string.IsNullOrWhiteSpace(scope) == true)
            {
                settings = _configuration.GetGlobalConfigurationAsNameValueCollection("metrics", "settings");
            }
            else
            {
                settings = _configuration.GetTenantConfigurationAsNameValueCollection(scope, "metrics", "settings");
            }

            return settings;
        }
        

        private IMeter CreateMeter(string scope, NameValueCollection settings)
        {
            int port = 0, mtu = 1432, batchingInterval = 1;
            var applyPrefix = settings["applyPrefix"] ?? string.Empty;
            var tenantPrefix = settings["prefix"] ?? string.Empty;

            var @namespace = string.Empty;
            // Should apply prefix if 
            var shouldApplyPrefix =
                string.IsNullOrWhiteSpace(applyPrefix) == true ||
                "|no|n|false|0|off|".IndexOf(applyPrefix, StringComparison.OrdinalIgnoreCase) == -1;
            var isGlobal = string.IsNullOrWhiteSpace(scope);
            if (isGlobal == false && shouldApplyPrefix == true)
            {
                if (string.IsNullOrWhiteSpace(tenantPrefix) == false)
                    @namespace = tenantPrefix + "." + _application;
                else
                    @namespace = scope + "." + _application;
            }
            else
                @namespace = _application;

            var host = settings["host"];
            if(string.IsNullOrWhiteSpace(host) == true )
                return NullMeter.Instance;
            if (int.TryParse(settings["port"], out port) == false)
                return NullMeter.Instance;
            EndPoint endpoint = null;
            IPAddress ip;
            if (IPAddress.TryParse(host, out ip) == true)
                endpoint = new IPEndPoint(ip, port);
            else
                endpoint = new DnsEndPoint(host, port);


            if (int.TryParse(settings["mtu"], out mtu) == false)
                mtu = 1432;
            if (int.TryParse(settings["batchingInterval"], out batchingInterval) == false)
                batchingInterval = 1;

            var key = $"{host}:{port}";
            return _meterCache.GetExistingOrCreate(key, () => new AsyncStatsDMeter(new StatsdMeter(endpoint, @namespace, mtu, batchingInterval)));


        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                
                if (disposing)
                {
                    var configCopy = _configCache;
                    if(configCopy != null )
                    {
                        _configCache = null;
                        configCopy.Dispose();
                    }
                    var meterCopy = _meterCache;
                    if(meterCopy != null )
                    {
                        _meterCache = null;
                        meterCopy.Dispose();
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
