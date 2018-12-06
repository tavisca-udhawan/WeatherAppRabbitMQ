using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.ExchangeRate.Entities
{
    public class Endpoint
    {
        public Uri Url { get; set; }

        public Dictionary<string, string> CustomHeaders { get; } = new Dictionary<string, string>();

        public int TimeOutInSeconds { get; set; }
    }
}
