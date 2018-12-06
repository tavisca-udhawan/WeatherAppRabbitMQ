using System;
using Tavisca.Platform.Common.ExceptionManagement;

namespace Tavisca.Platform.Common.Tests.Aws
{
    public class TestErrorHandler : IErrorHandler
    {
        public bool HandleException(Exception ex, string policy, out Exception newException)
        {
            newException = null;
            return false;
        }
    }
}
