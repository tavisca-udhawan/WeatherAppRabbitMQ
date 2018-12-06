using System;
using System.Collections.Concurrent;
using Tavisca.Platform.Common.Containers;
using System.IO;
using System.Collections.Generic;
#if NET_STANDARD
using Microsoft.Extensions.Configuration;
#endif

namespace Tavisca.Platform.Common.ConfigurationHandler
{
    public class ConfigurationManager
    {
        private static ConcurrentDictionary<string, string> _appSettings;

        public static string GetAppSetting(string key)
        {
            if (AppSettings != null && AppSettings.ContainsKey(key))
                return AppSettings[key];
            return null;
        }

        private static ConcurrentDictionary<string, string> AppSettings
        {
            get
            {
                if (_appSettings != null && _appSettings.Count > 0)
                    return _appSettings;
                _appSettings = new ConcurrentDictionary<string, string>();
#if NET_STANDARD
                var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var builder = new ConfigurationBuilder().SetBasePath(Path.Combine(AppContext.BaseDirectory))
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile($"appsettings.{environmentName}.json", optional: true);
                var configuration = builder.Build();

                Dictionary<string,string> section = new Dictionary<string, string>();
                configuration.GetSection("AppSetting").Bind(section);
                if (section == null)
                    throw new DependencyException("AppSetting section in file appsettings.json not found. Add file and section.");
                foreach (var data in section)
                {
                    _appSettings.GetOrAdd(data.Key, data.Value);
                }
#else
                var appSettings = System.Configuration.ConfigurationManager.AppSettings;
                if (appSettings != null)
                {
                    foreach (var k in appSettings.AllKeys)
                    {
                        _appSettings.GetOrAdd(k, appSettings[k]);
                    }
                }
#endif
                if (_appSettings == null)
                    throw new DependencyException("Could not bind AppSetting section. Please check.");
                return _appSettings;
            }
        }
    }
}
