using System;

namespace Tavisca.Platform.Common.ExceptionManagement
{
    public interface IErrorHandler
    {
        bool HandleException(Exception ex, string policy, out Exception newException);
    }
}