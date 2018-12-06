using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tavisca.Platform.Common.Containers;

namespace Tavisca.Platform.Common.Core.ServiceLocator
{
   public class NetCoreServiceLocator: IServiceLocator
    {
        public NetCoreServiceLocator(IContainerFactory containerFactory, IModule[] modules = null)
        {
            Platform.Common.ObjectFactory.Initialize(containerFactory, modules);
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            var output = Platform.Common.ObjectFactory.GetAllInstances(serviceType);
            return Convert(output);
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return Platform.Common.ObjectFactory.GetAllInstances<TService>();
        }

        public object GetInstance(Type serviceType)
        {
            return Platform.Common.ObjectFactory.GetInstance(serviceType);
        }

        public object GetInstance(Type serviceType, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return this.GetInstance(serviceType);
            return Platform.Common.ObjectFactory.GetInstance(serviceType, key);
        }

        public TService GetInstance<TService>()
        {
            return Platform.Common.ObjectFactory.GetInstance<TService>();
        }

        public TService GetInstance<TService>(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return this.GetInstance<TService>();
            return Platform.Common.ObjectFactory.GetInstance<TService>(key);
        }

        public object GetService(Type serviceType)
        {
            return Platform.Common.ObjectFactory.GetInstance(serviceType);
        }

        private static IEnumerable<object> Convert(IEnumerable enumerable)
        {
            return enumerable.Cast<object>().ToList();
        }
    }
}
