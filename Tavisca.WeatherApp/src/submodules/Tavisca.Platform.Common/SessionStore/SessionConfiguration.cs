using System;

namespace Tavisca.Common.Plugins.SessionStore
{
    public class SessionConfiguration
    {
        public TimeSpan ExpiresIn { get; set; }

        public int MaxItemsPerAsyncQueue { get; set; }

    }
}
