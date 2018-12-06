using System;

namespace Tavisca.Platform.Common
{
    public static class Extensions
    {

        public static long ToUnixTimestamp(this DateTime datetimeValue)
        {
            var epochTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return (long)datetimeValue.ToUniversalTime().Subtract(epochTime).TotalMilliseconds;
        }        
    }
}