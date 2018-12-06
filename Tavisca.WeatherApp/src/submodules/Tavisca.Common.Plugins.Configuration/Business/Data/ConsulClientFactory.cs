using Consul;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Configuration
{
    internal sealed class ConsulClientFactory
    {
        private static volatile ConsulClient instance;
        private static object syncRoot = new object();

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
                            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder().SetBasePath(Path.Combine(AppContext.BaseDirectory)).AddJsonFile("appsettings.json", true, true).AddJsonFile($"appsettings.{environmentName}.json", optional: true);
                            var configuration = builder.Build();
                            var consulConnectionString = configuration.GetSection(Constants.ConsulConnectionString).Value;

                            if (string.IsNullOrWhiteSpace(consulConnectionString))
                                consulConnectionString = ConstructConnectionString(Constants.DefaultConsulConnection);

                            var consulConnectionUri = new Uri(consulConnectionString);
                            instance = new ConsulClient(consulConfig => { consulConfig.Address = consulConnectionUri;});
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
