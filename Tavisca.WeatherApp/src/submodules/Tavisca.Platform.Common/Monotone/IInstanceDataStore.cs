using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Monotone
{
    public interface IInstanceDataStore
    {
        Dictionary<string, InstanceInfo> GetInstanceList();
        void AddOrUpdate(InstanceInfo instanceInfo);

    }
}
