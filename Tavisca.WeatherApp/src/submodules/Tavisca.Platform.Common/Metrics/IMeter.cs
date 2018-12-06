using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Metrics
{
    public interface IMeter
    {

        /// <summary>
        /// Increments or decrements a value on the server. At each flush the current count is sent and reset to 0.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        /// <param name="sampleRate">Sample rate, used if not all events are sent, for example 1 out of 10 = 0.1.</param>
        void Counter(string name, long value, double sampleRate);

        /// <summary>
        /// Increments or decrements a value on the server. At each flush the current count is sent and reset to 0.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        void Counter(string name, long value);

        /// <summary>
        /// Arbitrary values, an instantaneous measurement of a value.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        void Gauge(string name, ulong value);

        /// <summary>
        /// The amount of time something took.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        void Timer(string name, ulong value);

        /// <summary>
        /// The amount of time something took.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        void Timer(string name, TimeSpan value);

        /// <summary>
        /// A meter measures the rate of events over time, calculated at the server. They may also be thought of as increment-only counters.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        void Meter(string name, ulong value);

        /// <summary>
        /// A meter measures the rate of events over time, calculated at the server. They may also be thought of as increment-only counters.
        /// </summary>
        /// <param name="name">Name.</param>
        void Meter(string name);

        /// <summary>
        /// A "set" collects unique values, ignoring duplicates, and flushes the count of those unique values.
        /// It's similar to a <see cref="System.Collections.Generic.HashSet{T}.Count"/>.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="value">Value.</param>
        void Set(string name, string value);
    }
}
