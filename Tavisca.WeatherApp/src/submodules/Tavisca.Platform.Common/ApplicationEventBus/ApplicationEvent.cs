using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.ApplicationEventBus
{
    [Serializable]
    public class ApplicationEvent
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Context { get; set; }

        public string Publisher { get; set; }

        public DateTime TimeStamp { get; set; }

    }
}
