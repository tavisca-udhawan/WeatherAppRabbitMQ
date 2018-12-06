using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.LockManagement;

namespace Tavisca.Common.Plugins.Redis
{
    public class RedisSettings 
    {
        public string QueueName { get; set; }

        public List<string> Hosts { get; } = new List<string>();

        public bool IsDisabled { get; set; }

        public int TimeoutInMs { get; set; } = 2000;

        public string Signature
        {
            get
            {
                // Calculate and assign signature
                var data = string.Join("--", this.Hosts);

                using (var sha = SHA512.Create())
                {
                    return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(data)));
                }
            }
        }
    }
}
