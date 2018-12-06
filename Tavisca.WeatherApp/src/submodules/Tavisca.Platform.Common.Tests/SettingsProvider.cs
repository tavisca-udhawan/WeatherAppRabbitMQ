using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Tests
{
    static class SettingsProvider
    {
        public static class RabbitMqSettings
        {
            public static string HostName
            {
                get
                {
                    return ConfigurationManager.AppSettings["RabbitHostName"];
                }
            }

            public static string VirtualHost
            {
                get
                {
                    return ConfigurationManager.AppSettings["VirtualHost"];
                }
            }

            public static string Username
            {
                get
                {
                    return ConfigurationManager.AppSettings["RabbitUsername"];
                }
            }

            public static string Password
            {
                get
                {
                    return ConfigurationManager.AppSettings["RabbitPassword"];
                }
            }

            public static int Port
            {
                get
                {
                    return int.Parse(ConfigurationManager.AppSettings["RabbitPort"]);
                }
            }
        }
    }
}
