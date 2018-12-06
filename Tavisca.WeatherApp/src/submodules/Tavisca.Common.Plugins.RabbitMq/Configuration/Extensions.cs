using System;
using System.Collections.Generic;
using System.Text;

namespace Tavisca.Common.Plugins.RabbitMq
{
    public static class Extensions
    {
        public static string GetQueueIdentifier(this Queue queue)
        {
            var readQueue = queue as ReadQueue;
            if (readQueue != null)
                return readQueue.Name;
            var writeQueue = queue as WriteQueue;
            if (writeQueue != null)
                return writeQueue.RoutingKey;
            return null;
        }
    }
}
