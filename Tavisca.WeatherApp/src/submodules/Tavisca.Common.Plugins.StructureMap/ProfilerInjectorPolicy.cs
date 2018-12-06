using System;
using System.Linq;
using System.Collections.Generic;
using StructureMap.Building.Interception;
using StructureMap.Pipeline;
using Castle.DynamicProxy;
using Tavisca.Platform.Common.Profiling;
using System.Reflection;
using Tavisca.Platform.Common.Context;

namespace Tavisca.Common.Plugins.StructureMap
{
    public class ProfilerInjectorPolicy : IInterceptorPolicy
    {
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();
        private static readonly MethodInfo GenericProxyDelegate = typeof(ProfilerInjectorPolicy).GetMethod("GetProxy", BindingFlags.NonPublic | BindingFlags.Static);
        public string Description
        {
            get
            {
                return "Profiler injector policy";
            }
        }

        public IEnumerable<global::StructureMap.Building.Interception.IInterceptor> DetermineInterceptors(Type pluginType, Instance instance)
        {
#if NET_STANDARD
            var isTaviscaNamespace = pluginType.FullName.StartsWith("Tavisca") || pluginType.FullName.StartsWith("CnxLoyalty");
            if (isTaviscaNamespace)
            {
                var interceptorType = typeof(FuncInterceptor<>).MakeGenericType(pluginType);
                var method = GenericProxyDelegate.MakeGenericMethod(pluginType);
                var arg = System.Linq.Expressions.Expression.Parameter(pluginType, "x");
                var methodCall = System.Linq.Expressions.Expression.Call(method, arg);
                var delegateType = typeof(Func<,>).MakeGenericType(pluginType, pluginType);
                var lambda = System.Linq.Expressions.Expression.Lambda(delegateType, methodCall, arg);
                var interceptor = Activator.CreateInstance(interceptorType, lambda, string.Empty);

                yield return interceptor as global::StructureMap.Building.Interception.IInterceptor;
            }
#else
            var interceptorType = typeof(FuncInterceptor<>).MakeGenericType(pluginType);
            var method = GenericProxyDelegate.MakeGenericMethod(pluginType);
            var arg = System.Linq.Expressions.Expression.Parameter(pluginType, "x");
            var methodCall = System.Linq.Expressions.Expression.Call(method, arg);
            var delegateType = typeof(Func<,>).MakeGenericType(pluginType, pluginType);
            var lambda = System.Linq.Expressions.Expression.Lambda(delegateType, methodCall, arg);
            var interceptor = Activator.CreateInstance(interceptorType, lambda, string.Empty);

            yield return interceptor as global::StructureMap.Building.Interception.IInterceptor;
#endif
        }

        private static T GetProxy<T>(object service)
        {
#if NET_STANDARD
            var typeToIntercept = typeof(T);
            var isTaviscaNamespace = typeToIntercept.FullName.StartsWith("Tavisca") || typeToIntercept.FullName.StartsWith("CnxLoyalty");
            if (typeToIntercept.GetTypeInfo().IsInterface && isTaviscaNamespace)
            {
                var allInterfaces = service.GetType().GetInterfaces();
                var additionalInterfaces = allInterfaces.Except(new[] { typeToIntercept }).ToArray();
                var result = ProxyGenerator.CreateInterfaceProxyWithTargetInterface(typeToIntercept,
                    additionalInterfaces, service, new ProfilerInterceptor());
                return (T)result;
            }

            return (T)service;
#else
            var typeToIntercept = typeof (T);
            if (typeToIntercept.GetTypeInfo().IsInterface)
            {
                var allInterfaces = service.GetType().GetInterfaces();
                var additionalInterfaces = allInterfaces.Except(new[] {typeToIntercept}).ToArray();
                var result = ProxyGenerator.CreateInterfaceProxyWithTargetInterface(typeToIntercept,
                    additionalInterfaces, service, new ProfilerInterceptor());
                return (T) result;
            }

            return (T) service;
#endif
        }
    }
}
