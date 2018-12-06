using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.RabbitMq
{
    public abstract class Queue
    {
        public static readonly IEqualityComparer<Queue> QueueComparer = new BasicQueueComparer();

        public string Host { get; set; }

        public List<string> SecondaryHosts { get; } = new List<string>();

        public string Username { get; set; }

        public string Password { get; set; }

        public string VirtualHost { get; set; }

        public int Port { get; set; }
        
        public override bool Equals(object obj)
        {
            var otherQueue = obj as Queue;

            if (otherQueue == null)
                return false;

            return Username.Equals(otherQueue.Username)
                    && Password.Equals(otherQueue.Password)
                    && VirtualHost.Equals(otherQueue.VirtualHost, StringComparison.OrdinalIgnoreCase)
                    && Host.Equals(otherQueue.Host, StringComparison.OrdinalIgnoreCase)
                    && Port.Equals(otherQueue.Port);
        }
        public override int GetHashCode()
        {
            unchecked //ignore overflow
            {
                int hash = (int)2166136261;
                hash = hash * 16777619 ^ (Username ?? string.Empty).GetHashCode();
                hash = hash * 16777619 ^ (Password ?? string.Empty).GetHashCode();
                hash = hash * 16777619 ^ Port.GetHashCode();
                hash = hash * 16777619 ^ StringComparer.OrdinalIgnoreCase.GetHashCode(VirtualHost ?? string.Empty);
                hash = hash * 16777619 ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Host ?? string.Empty);
                return hash;
            }
        }
        private class BasicQueueComparer : IEqualityComparer<Queue>
        {
            public bool Equals(Queue x, Queue y)
            {
                if (x == null || y == null)
                    return false;
                return x.Username.Equals(y.Username)
                    && x.Password.Equals(y.Password)
                    && x.VirtualHost.Equals(y.VirtualHost, StringComparison.OrdinalIgnoreCase)
                    && x.Host.Equals(y.Host, StringComparison.OrdinalIgnoreCase)
                    && x.Port.Equals(y.Port);
            }

            public int GetHashCode(Queue obj)
            {
                unchecked // Overflow is fine, just wrap
                {
                    int hash = (int)2166136261;
                    hash = hash * 16777619 ^ (obj.Username ?? string.Empty).GetHashCode();
                    hash = hash * 16777619 ^ (obj.Password ?? string.Empty).GetHashCode();
                    hash = hash * 16777619 ^ obj.Port.GetHashCode();
                    hash = hash * 16777619 ^ StringComparer.OrdinalIgnoreCase.GetHashCode(obj.VirtualHost ?? string.Empty);
                    hash = hash * 16777619 ^ StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Host ?? string.Empty);
                    return hash;
                }
            }
        }

        internal IConnection CreateConnection()
        {
            var hosts = new List<string> { Host };
            hosts.AddRange(SecondaryHosts);
            var factory = new ConnectionFactory()
            {
                VirtualHost = VirtualHost,
                UserName = Username,
                Password = Password,
                Port = Port,
                AutomaticRecoveryEnabled = true
            };
            return factory.CreateConnection(hosts);
        }
    }


    public class ReadQueue : Queue
    {
        public ushort PrefetchSize { get; set; } = 1;

        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            var otherQueue = obj as ReadQueue;

            if (otherQueue == null)
                return false;
            
            return base.Equals(obj) && 
                PrefetchSize.Equals(otherQueue.PrefetchSize)
                && Name.Equals(otherQueue.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            unchecked //ignore overflow
            {
                int hash = (int)2166136261;
                hash = base.GetHashCode();
                hash = hash * 16777619 ^ PrefetchSize.GetHashCode();
                hash = hash * 16777619 ^ StringComparer.OrdinalIgnoreCase.GetHashCode(Name ?? string.Empty);
              
                return hash;
            }
        }

        public ReadQueue Clone()
        {
            return new ReadQueue
            {
                Host = this.Host,
                Name = this.Name,
                Password = this.Password,
                Port = this.Port,
                PrefetchSize = this.PrefetchSize,
                Username = this.Username,
                VirtualHost = this.VirtualHost
            };
        }
    }

    public class WriteQueue : Queue
    {
        public string Exchange { get; set; }

        public string RoutingKey { get; set; }
    }
}
