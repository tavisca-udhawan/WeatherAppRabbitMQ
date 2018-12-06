using Newtonsoft.Json;
using System;
using System.Diagnostics;

namespace Tavisca.Platform.Common.Profiling
{
    public class PerformanceLog
    {
        public PerformanceLog()
        {
            StartTimeStamp = Stopwatch.GetTimestamp();
        }
        [JsonIgnore]
        internal long StartTimeStamp { get; set; }
        public string Info { get; set; }

        public float StartPointInMilliseconds
        {
            get
            {
                return (StartTimeStamp / (float)Stopwatch.Frequency) * 1000;
            }
        }
        public TimeSpan? TotalExecutionTime
        {
            get; set;
        }
    }
}
