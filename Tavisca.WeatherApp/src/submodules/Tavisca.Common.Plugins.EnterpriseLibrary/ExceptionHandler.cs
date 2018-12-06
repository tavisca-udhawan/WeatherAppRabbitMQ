using System;
using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
using Tavisca.Platform.Common.ExceptionManagement;

namespace Tavisca.Common.Plugins.EnterpriseLibrary
{
    public class ExceptionHandler : IErrorHandler
    {
        public ExceptionHandler(IEnumerable<ExceptionPolicyDefinition> definitions)
        {
            _manager = new ExceptionManager(definitions);
        }

        private readonly ExceptionManager _manager;

        public bool HandleException(Exception ex, string policy, out Exception newException)
        {
            return _manager.HandleException(ex, policy, out newException);
        }
    }
}
