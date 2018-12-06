using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Monotone
{
    internal class IdGenerator
    {
        public IdGenerator(uint regionId, uint zoneId, uint instanceId)
        {
            Region = regionId;
            Zone = zoneId;
            Instance = instanceId;
        }

        public static readonly int RegionBits = 4;

        public static readonly int ZoneBits = 3;

        public static readonly int InstanceBits = 10;

        public uint Region { get; private set; }

        public uint Zone { get; private set; }

        public uint Instance { get; private set; }

        public static readonly long DefaultLagInMilliseconds = 10000;

        public static readonly DateTime Epoch = new DateTime(2016, 4, 5);

        private static long _previous = 0;

        private static readonly object _syncRoot = new object();

        public BigId NextId()
        {
            var current = Convert.ToInt64(DateTime.Now.Subtract(Epoch).TotalMilliseconds);

            long delta = 0;

            lock (_syncRoot)
            {
                // If current is more than previous then this is the first request within the current millisecond.
                // Hence ee can generate the id from it.
                if (current > _previous)
                {
                    _previous = current;
                    return CreateId(current);
                }
                else
                    delta = _previous - current;
            }

            if (delta > DefaultLagInMilliseconds)
                throw new Exception("System clock has moved behind recorded time.");

            if (delta == 0)
                Thread.Sleep(1);
            else
                Thread.Sleep(Convert.ToInt32(delta));

            return NextId();
        }

        internal BigId CreateId(long current)
        {
            Zone = Zone & GetMask(ZoneBits);
            Region = Region & GetMask(RegionBits);
            Instance = Instance & GetMask(InstanceBits);
            var adjustedCurrent = current & GetLongMask(64 - InstanceBits - ZoneBits - RegionBits);
            if (adjustedCurrent != current)
                throw new Exception("Id generator max id value exceeded.");
            var id = 
                (current << (InstanceBits + ZoneBits + RegionBits)) + 
                (Region << (InstanceBits + ZoneBits)) + 
                (Zone << InstanceBits) + 
                Instance;
            return new BigId(id);
        }

        private uint GetMask(int oneBits)
        {
            return (uint.MaxValue << oneBits) ^ uint.MaxValue;
        }

        private long GetLongMask(int oneBits)
        {
            return (long.MaxValue << oneBits) ^ long.MaxValue;
        }
    }
}
