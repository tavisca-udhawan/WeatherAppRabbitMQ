using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common;
using Tavisca.Platform.Common.Metrics;

namespace Tavisca.Common.Plugins.StatsD
{
    public class AsyncStatsDMeter : IMeter
    {
        IMeter _meter = null;

        public AsyncStatsDMeter(IMeter meter)
        {
            _meter = meter;
        }

        public void Counter(string name, long value)
        {
            AsyncTasks.Run(() => _meter.Counter(name, value), "metering");
        }

        public void Counter(string name, long value, double sampleRate)
        {
            AsyncTasks.Run(() => _meter.Counter(name, value, sampleRate), "metering");
        }

        public void Gauge(string name, ulong value)
        {
            AsyncTasks.Run(() => _meter.Gauge(name, value), "metering");
        }

        public void Meter(string name)
        {
            AsyncTasks.Run(() => _meter.Meter(name), "metering");
        }

        public void Meter(string name, ulong value)
        {
            AsyncTasks.Run(() => _meter.Meter(name, value), "metering");
        }

        public void Set(string name, string value)
        {
            AsyncTasks.Run(() => _meter.Set(name, value), "metering");
        }

        public void Timer(string name, TimeSpan value)
        {
            AsyncTasks.Run(() => _meter.Timer(name, value), "metering");
        }

        public void Timer(string name, ulong value)
        {
            AsyncTasks.Run(() => _meter.Timer(name, value), "metering");
        }
    }
}
