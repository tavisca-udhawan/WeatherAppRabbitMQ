using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.Metrics;

namespace Tavisca.Common.Plugins.StatsD
{
    public class StatsdMeter : IMeter
    {
        public StatsdMeter(EndPoint serverEndPoint, string metricsNamespace, int mtu = 1432, int batchingIntervalInSeconds = 1)
        {
            _client = new StatsdClient(serverEndPoint, metricsNamespace, mtu);
        }

        private readonly IBatchingClient _client;

        public string MetricsNamespace
        {
            get
            {
                return _client.MetricsNamespace;
            }
        }

        public void Counter(string name, long value, double sampleRate)
        {
            _client.Counter(name, value, sampleRate);
        }

        public void Counter(string name, long value)
        {
            _client.Counter(name, value);
        }

        public void Gauge(string name, ulong value)
        {
            _client.Gauge(name, value);
        }

        public void Timer(string name, ulong value)
        {
            _client.Timer(name, value);
        }

        public void Timer(string name, TimeSpan value)
        {
            _client.Timer(name, value);
        }

        public void Meter(string name, ulong value)
        {
            _client.Meter(name, value);
        }

        public void Meter(string name)
        {
            _client.Meter(name);
        }

        public void Set(string name, string value)
        {
            _client.Set(name, value);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
