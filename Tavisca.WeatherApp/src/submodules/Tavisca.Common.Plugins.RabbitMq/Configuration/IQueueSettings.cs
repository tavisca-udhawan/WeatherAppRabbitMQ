using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.RabbitMq
{
    public interface IQueueSettings
    {
        Task<ReadQueue> GetReadQueueAsync(string topic);

        Task<WriteQueue> GetWriteQueueAsync(string topic);
    }
}
