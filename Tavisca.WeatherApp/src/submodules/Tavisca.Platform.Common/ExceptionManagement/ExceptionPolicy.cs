using System;
using Tavisca.Platform.Common.ExceptionManagement;

namespace Tavisca.Platform.Common
{
    public static class ExceptionPolicy
    {
        public static void Configure(IErrorHandler handler)
        {
            _handler = handler;
        }

        private static IErrorHandler _handler = null;

        public static bool HandleException(Exception ex, string policy, out Exception newException)
        {
            if( _handler == null )
                throw new Exception("Exception policy is not initialized. Use ExceptionPolicy.Configure() to initialize the policy before use.");
            return _handler.HandleException(ex, policy, out newException);
        }

        public static void HandleException(Exception ex, string policy = null)
        {
            if (_handler == null)
                throw new Exception("Exception policy is not initialized. Use ExceptionPolicy.Configure() to initialize the policy before use.");
            Exception newException = null;
            var reThrow = _handler.HandleException(ex, policy ?? Policies.DefaultPolicy, out newException);
            if (reThrow)
                throw newException;
        }
    }
}
