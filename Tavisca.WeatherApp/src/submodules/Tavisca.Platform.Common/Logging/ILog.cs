using System;
using System.Collections.Generic;

namespace Tavisca.Platform.Common.Logging
{
    public interface ILog
    {
        string Id { get; }

        DateTime LogTime { get; }

        IEnumerable<KeyValuePair<string, object>> GetFields();

        List<ILogFilter> Filters { get; }
    }
}