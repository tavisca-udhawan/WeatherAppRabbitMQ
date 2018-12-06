using System;

namespace Tavisca.Common.Plugins.Aerospike
{
    internal static class Utility
    {
        public static int GetPartitionCount(int itemCount, int queueLength)
        {
            var partitionCount = (int)(Math.Ceiling((float)itemCount / queueLength));
            if (partitionCount == 0)
            {
                if (itemCount > 5)
                    partitionCount = 2;
                else
                    partitionCount = 1;
            }
            return partitionCount;
        }
    }
}
