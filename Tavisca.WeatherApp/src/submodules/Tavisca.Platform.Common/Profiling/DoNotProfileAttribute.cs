using System;

namespace Tavisca.Platform.Common.Profiling
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DoNotProfileAttribute : Attribute
    {
    }
}
