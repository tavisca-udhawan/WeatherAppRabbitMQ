using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Monotone
{
    internal class SingleMachineConfiguration : IIdConfiguration
    {
        public uint GetInstanceId()
        {
            return 0;
        }

        public uint GetRegionId()
        {
            return 0;
        }

        public uint GetZoneId()
        {
            return 0;
        }

        public void Intialize()
        {
            // Do nothing
        }
    }
}
