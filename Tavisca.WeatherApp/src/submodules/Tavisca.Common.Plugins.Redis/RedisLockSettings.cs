using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Tavisca.Platform.Common.LockManagement;

namespace Tavisca.Common.Plugins.Redis
{
    public class RedisLockSettings : RedisSettings,ILockSettings
    {

        public int LockTimeOut { get; set; }

        public int RetryTimeOut { get; set; }

    }
}