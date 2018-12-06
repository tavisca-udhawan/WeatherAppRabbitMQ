using System;
using Castle.DynamicProxy;
using System.Threading.Tasks;
using System.Diagnostics;
using Tavisca.Platform.Common.Context;
using System.Reflection;

namespace Tavisca.Platform.Common.Profiling
{
    public class ProfilerInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            if (!IsProfilingEnabled(invocation))
                invocation.Proceed();
            else
                DoProfilingWrappedInvocation(invocation);
        }

        private void DoProfilingWrappedInvocation(IInvocation invocation)
        {
            var sw = new Stopwatch();
            var callInfo = string.Format("Calling {0}.{1}", invocation.TargetType.Name, invocation.MethodInvocationTarget.Name);
            var profileContext = new ProfileContext(callInfo);
            sw.Start();
            try
            {
                invocation.Proceed();
            }
            finally
            {

                if (IsMethodAsync(invocation.Method.ReturnType))
                {
                    var task = invocation.ReturnValue as Task;
                    task.ContinueWith(x =>
                    {
                        profileContext.Dispose();
                    }, TaskContinuationOptions.ExecuteSynchronously);
                }
                else
                    profileContext.Dispose();
            }
        }

        private bool IsMethodAsync(Type returnType)
        {
            if (returnType == null)
                return false;

            return typeof(Task).GetTypeInfo().IsAssignableFrom(returnType);
        }

        private static bool IsProfilingEnabled(IInvocation invocation)
        {
            if (invocation.Method.IsDefined(typeof(DoNotProfileAttribute), true) == true)
                return false;
            return CallContext.Current != null
                && CallContext.Current.IsProfilingEnabled;
        }
    }
}
