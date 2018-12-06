using System;

namespace Tavisca.Common.Plugins.Redis
{
    public static class Keystore
    {

        public static class RedisKeys
        {
           
            public const string RedisSettings = "redis_settings";
            public const string LockSettings = "lock_settings";
            public const string CacheSettings = "cache_settings";
            public static string Error = "Error occured in redis Provider";

            public static class Errors
            {
                internal static string DeleteKeyFromCache(string key)
                {
                    return string.Format("Failed to delete key '{0}' from cache.", key);
                }

                internal static string GetKeyFromCache(string key)
                {
                    return string.Format("Failed to get key '{0}' from cache.", key);
                }

                internal static string SetKeyFromCache(string key)
                {
                    return string.Format("Failed to set key '{0}' in cache.", key);
                }

                internal static string MultiGetKeyFromCache()
                {
                    return "Multi get from cache failed.";
                }
            }
        }

    }
}
