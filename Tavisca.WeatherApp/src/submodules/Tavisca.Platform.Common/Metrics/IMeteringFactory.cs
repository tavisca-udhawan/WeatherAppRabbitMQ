using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Configurations;

namespace Tavisca.Platform.Common.Metrics
{
    public interface IMeteringFactory : IDisposable
    {
        IMeter CreateNew(string scope = null);
    }
}
