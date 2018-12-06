using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Logging.Fluent
{
    public class DelegateFilterConfiguration
    {
        internal ILoggingHttpFilter _filter;

        internal Func<ILog, ILog> _mask;

        public DelegateFilterConfiguration(ILoggingHttpFilter filter, Func<ILog, ILog> mask)
        {
            _filter = filter;
            _mask = mask;
        }

        public ILoggingHttpFilter Apply()
        {
            _filter.Filters.Add(new DelegateFilter(_mask));
            return _filter;
        }
    }

    public class DelegateFilter : ILogFilter
    {
        private Func<ILog, ILog> _mask;

        public DelegateFilter(Func<ILog, ILog> mask)
        {
            _mask = mask;
        }

        public ILog Apply(ILog log)
        {
            return _mask(log);
        }
    }
}
