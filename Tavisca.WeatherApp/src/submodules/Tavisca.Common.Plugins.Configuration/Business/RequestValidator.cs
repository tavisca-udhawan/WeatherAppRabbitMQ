using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Configuration
{
    internal static class RequestValidator
    {
        public static void ValidateGlobal(string applicationName, string sectionName, string key)
        {

            StringValidator.ValidateIsNullOrEmpty(applicationName, nameof(applicationName));
            StringValidator.ValidateIsNullOrEmpty(sectionName, nameof(sectionName));
            StringValidator.ValidateIsNullOrEmpty(key, nameof(key));

        }

        public static void ValidateGlobal(string sectionName, string key)
        {

            StringValidator.ValidateIsNullOrEmpty(sectionName, nameof(sectionName));
            StringValidator.ValidateIsNullOrEmpty(key, nameof(key));

        }

        public static void ValidateTenant(string tenantId, string sectionName, string key)
        {

            StringValidator.ValidateIsNullOrEmpty(tenantId, nameof(tenantId));
            StringValidator.ValidateIsNullOrEmpty(sectionName, nameof(sectionName));
            StringValidator.ValidateIsNullOrEmpty(key, nameof(key));

        }

        public static void ValidateTenant(string tenantId, string applicationName, string sectionName, string key)
        {

            StringValidator.ValidateIsNullOrEmpty(tenantId, nameof(tenantId));
            StringValidator.ValidateIsNullOrEmpty(sectionName, nameof(sectionName));
            StringValidator.ValidateIsNullOrEmpty(key, nameof(key));
            StringValidator.ValidateIsNullOrEmpty(applicationName, nameof(applicationName));

        }
    }
}
