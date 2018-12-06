using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.Metrics;

namespace Tavisca.Platform.Common.Metrics
{
    public class NullMeter : IMeter
    {
        public static IMeter Instance = new NullMeter();

        private NullMeter()
        {
        }

        public void Counter(string name, long value)
        {
            // Do nothing
        }

        public void Counter(string name, long value, double sampleRate)
        {
            // Do nothing
        }

        public void Dispose()
        {
            // Do nothing
        }

        public void Gauge(string name, ulong value)
        {
            // Do nothing
        }

        public void Meter(string name)
        {
            // Do nothing
        }

        public void Meter(string name, ulong value)
        {
            // Do nothing
        }

        public void Set(string name, string value)
        {
            // Do nothing
        }

        public void Timer(string name, TimeSpan value)
        {
            // Do nothing
        }

        public void Timer(string name, ulong value)
        {
            // Do nothing
        }
    }
}
