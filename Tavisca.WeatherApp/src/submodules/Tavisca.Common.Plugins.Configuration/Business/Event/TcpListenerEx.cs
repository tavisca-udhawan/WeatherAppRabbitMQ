using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Configuration
{
    public class TcpListenerEx : TcpListener
    {
        public TcpListenerEx(IPAddress ipAddress, int port) : base(ipAddress, port) { }

        public TcpListenerEx(IPEndPoint ipEndPoint) : base(ipEndPoint) { }

        public bool IsActive
        {
            get
            {
                return base.Active;
            }
        }


    }
}
