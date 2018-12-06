using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Containers
{
    public interface IContainerFactory
    {
        IDependencyContainer CreateContainer(IEnumerable<Registration> registrations);
    }
}
