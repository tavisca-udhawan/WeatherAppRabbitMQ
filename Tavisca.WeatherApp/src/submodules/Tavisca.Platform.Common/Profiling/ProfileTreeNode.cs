using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Tavisca.Platform.Common.Profiling
{
    public class ProfileTreeNode
    {
        public PerformanceLog PerformanceLog { get; set; }
        public LinkedList<ProfileTreeNode> ChildNodes { get; } = new LinkedList<ProfileTreeNode>();
        [JsonIgnore]
        public ProfileTreeNode ParentNode { get; set; }
        public TimeSpan? SelfTime
        {
            get
            {
                if (PerformanceLog?.TotalExecutionTime != null)
                {
                    var childTimes = ChildNodes.Select(x => x.PerformanceLog?.TotalExecutionTime).Where(x => x != null).ToList();
                    return childTimes.Any() ?
                                         PerformanceLog.TotalExecutionTime - childTimes.Aggregate((x, y) => x + y)
                                         : PerformanceLog.TotalExecutionTime;
                }

                return null;
            }
        }

        public TimeSpan? TotalChildrenTime
        {
            get
            {
                if (ChildNodes.Any())
                {
                    var childTimes = ChildNodes.Select(x => x.PerformanceLog?.TotalExecutionTime).Where(x => x != null).ToList();
                    return childTimes.Any() ?
                                         childTimes.Aggregate((x, y) => x + y)
                                         : null;
                }

                return null;
            }
        }
    }
}
