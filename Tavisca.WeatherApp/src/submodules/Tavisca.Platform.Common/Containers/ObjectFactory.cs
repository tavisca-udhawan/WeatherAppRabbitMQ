using System;
using System.Collections.Generic;
using System.Linq;
using Tavisca.Platform.Common.Containers;

namespace Tavisca.Platform.Common
{
    public static class ObjectFactory
    {
        private static IDependencyContainer _container = null;

        public static void Initialize( IContainerFactory containerFactory, IModule[] modules = null )
        {
            IEnumerable<Registration> registrations = Enumerable.Empty<Registration>();
            if (modules != null)
                registrations = modules.SelectMany(m => m.GetRegistrations());
            _container = containerFactory.CreateContainer(registrations);
        }

        public static IDependencyContainer GetContainer()
        {
            if (_container == null)
                throw new Exception("ObjectFactory is not intialized. Use ObjectFactory.Initialize() to setup the factory before use.");
            return _container;
        }

        public static IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return GetContainer().GetAllInstances(serviceType);
        }
        public static IEnumerable<TService> GetAllInstances<TService>()
        {
            return GetContainer().GetAllInstances<TService>();
        }
        public static object GetInstance(Type serviceType)
        {
            return GetContainer().GetInstance(serviceType);
        }

        public static object GetInstance(Type serviceType, string key)
        {
            return GetContainer().GetInstance(serviceType, key);
        }

        public static TService GetInstance<TService>()
        {
            return GetContainer().GetInstance<TService>();
        }
        public static TService GetInstance<TService>(string key)
        {
            return GetContainer().GetInstance<TService>(key);
        }
    }
}
