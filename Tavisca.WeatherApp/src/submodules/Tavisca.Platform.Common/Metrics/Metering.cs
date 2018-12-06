using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.ConfigurationHandler;
using Tavisca.Platform.Common.Configurations;
using Tavisca.Platform.Common.Metrics;

namespace Tavisca.Platform.Common.Metrics
{
    public static class Metering
    {
        private static IMeteringFactory _factory;

        public static void ConfigureMetrics(IMeteringFactory providerFactory)
        {
            _factory = providerFactory;
        }

        public static IMeter GetMeterForTenant(string tenant)
        {
            if (_factory == null)
                return NullMeter.Instance;
            else
                return GetMeterForTenant(tenant, _factory);
        }

        public static IMeter GetMeterForTenant(string tenant, IMeteringFactory factory)
        {
            try
            {
                if (IsDisabled() == true)
                    return NullMeter.Instance;
                return _factory.CreateNew(tenant);
            }
            catch
            {
                return NullMeter.Instance;
            }
        }

        public static IMeter GetGlobalMeter()
        {
            if (_factory == null)
                return NullMeter.Instance;
            else
                return GetGlobalMeter(_factory);
        }

        public static IMeter GetGlobalMeter(IMeteringFactory factory)
        {
            try
            {
                if (IsDisabled() == true)
                    return NullMeter.Instance;
                return _factory.CreateNew();
            }
            catch
            {
                return NullMeter.Instance;
            }
        }


        private static bool IsDisabled()
        {
            try
            {
                var disableMetrics = ConfigurationManager.GetAppSetting("disable-metrics") ?? "no";
                if (string.IsNullOrWhiteSpace(disableMetrics) == false && "|y|on|yes|true|1|".IndexOf(disableMetrics) != -1)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        } 

    }

    
}
