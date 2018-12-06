using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tavisca.Platform.Common.Monotone;

namespace Tavisca.Platform.Common
{
    public static class UniqueId
    {
        public static void Initialize(IIdConfiguration configuration)
        {
            if (_generator != null)
                throw new Exception("Monotone is already initialized.");
            _generator = new IdGenerator( configuration.GetRegionId(), configuration.GetZoneId(), configuration.GetInstanceId() ) { };
        }

        public static void InitializeForSingleMachine()
        {
            Initialize(new SingleMachineConfiguration());
        }

        private static IdGenerator _generator = null;
        
        public static BigId NextId()
        {
            if (_generator == null)
                throw new Exception("Monotone is not initialized.");
            return _generator.NextId();
        }
    }
}
