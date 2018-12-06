using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Logging.Fluent;

namespace Tavisca.Platform.Common.Logging
{
    public static class LoggingHttpFilterExtensions
    {
        public static FilterStateConfiguration ConfigureMasking(this ILoggingHttpFilter filter)
        {
            var builder = new FilterStateConfiguration(filter);
            return builder;
        }

        public static DelegateFilterConfiguration ConfigureMaskingDelegate(this ILoggingHttpFilter filter, Func<ILog, ILog> logfilter)
        {
            return new DelegateFilterConfiguration(filter, logfilter);
        }
    }
}
