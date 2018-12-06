using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Profiling
{
    public static class Profiling
    {
        public static void Trace(string message)
        {
            using (new ProfileContext($"Trace: {message}"))
            {
                // Do nothing
            }
        }
    }
}
