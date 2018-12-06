using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Serialization;
using Tavisca.Platform.Common.ServiceBus;



namespace Tavisca.Common.Plugins.RabbitMq
{
    internal interface ISubscription : IDisposable
    {
        void Notify(IBus bus, BasicDeliverEventArgs msg);

        ISubscription CreateSubscription(ReadQueue settings);
        
        void Stop();
    }
}
