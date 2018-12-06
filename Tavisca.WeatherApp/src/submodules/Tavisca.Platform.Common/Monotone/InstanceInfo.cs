using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Monotone
{
    public class InstanceInfo
    {
        public InstanceInfo()
        {

        }
        public InstanceInfo(uint regionId, uint zoneId, uint instanceId, DateTime updateTimestamp)
        {
            RegionId = regionId;
            ZoneId = zoneId;
            InstanceId = instanceId;
            UpdateTimestamp = updateTimestamp;
        }
        public uint RegionId { get; set; }

        public uint ZoneId { get; set; }

        public uint InstanceId { get; set; }

        public DateTime UpdateTimestamp { get; set; }

        public string GetUniqueId()
        {
            return string.Format("{0}-{1}-{2}", RegionId, ZoneId, InstanceId);
        }


    }
}
