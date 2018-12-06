using Consul;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Configuration
{
    internal sealed class ConsulClientFactory
    {
        private static volatile ConsulClient instance;
        private static object syncRoot = new Object();

        private ConsulClientFactory() { }

        public static ConsulClient Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            var connectionString = ConstructConnectionString(ConfigurationManager.AppSettings[Constants.ConsulConnectionString] ?? Constants.DefaultConsulConnection);
                            instance = new ConsulClient(new ConsulClientConfiguration() { Address = new Uri(connectionString) });

                        }
                    }
                }

                return instance;
            }
        }

        private static string ConstructConnectionString(string consulConnectionString)
        {
            if (!consulConnectionString.Contains(Constants.HttpString))
                return Constants.HttpString + consulConnectionString;
            else
                return consulConnectionString;


        }
    }
}
