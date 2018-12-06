using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Configuration
{
    internal class KeyMaker
    {
        internal static string ConstructKey(string scope, string applicationName, string section, string key)
        {

            if (string.Equals(scope, Constants.Global, StringComparison.OrdinalIgnoreCase))
                return string.Format(Constants.GlobalKey, scope, applicationName, section, key);
            else if (string.Equals(scope, Constants.Default, StringComparison.OrdinalIgnoreCase))
                return string.Format(Constants.DefaultKey, scope, applicationName, section, key);
            else
                return string.Format(Constants.TenantKey, scope, applicationName, section, key);

        }
    }
}
