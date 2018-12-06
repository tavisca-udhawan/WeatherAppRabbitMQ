using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StructureMap;
using StructureMap.Pipeline;
using Tavisca.Platform.Common.Containers;
#if NET_STANDARD
using Microsoft.Extensions.DependencyInjection;
#endif

namespace Tavisca.Common.Plugins.StructureMap
{
    public class ContainerFactory : IContainerFactory
    {
        private readonly Container _container = new Container();

        public ContainerFactory()
        {

        }

#if NET_STANDARD
        public ContainerFactory(IServiceCollection serviceCollection)
        {
            _container.Populate(serviceCollection);
        }
#endif

        public IDependencyContainer CreateContainer( IEnumerable<Registration> registrations  )
        {

            var registy = CreateRegistry(registrations);
            _container.Configure(congif => congif.AddRegistry(registy));
            return new DependencyContainer(_container);
        }

        private Registry CreateRegistry(IEnumerable<Registration> registrations )
        {
            var registry = new Registry();
            if (registrations != null)
            {
                foreach (var registration in registrations)
                {
                    Apply(registry, registration);
                }
            }
            return registry;
        }

        private void Apply(Registry registry, Registration registration)
        {
            if (registration.Constructor != null)
                ApplyWithConstructor(registry, registration);
            else 
                ApplyWithType(registry, registration);
        }

        private void ApplyWithConstructor(Registry registry, Registration registration)
        {
            var isNamePresent = string.IsNullOrWhiteSpace(registration.Name) == false;
            var isSingleton = registration.IsSingleton;
            if (isNamePresent && isSingleton)
                registry.For(registration.InterfaceType).Add(registration.Constructor()).Named(registration.Name).Singleton();
            else if (isNamePresent)
                registry.For(registration.InterfaceType).Add(registration.Constructor()).Named(registration.Name);
            else if (isSingleton)
                registry.For(registration.InterfaceType).Use(registration.Constructor()).Singleton();
            else
                registry.For(registration.InterfaceType).Use(registration.Constructor());
        }

        private void ApplyWithType(Registry registry, Registration registration)
        {
            var isNamePresent = string.IsNullOrWhiteSpace(registration.Name) == false;
            var isSingleton = registration.IsSingleton;
            if (isNamePresent && isSingleton)
                registry.For(registration.InterfaceType).Add(registration.ImplementationType).Named(registration.Name).Singleton();
            else if (isNamePresent)
                registry.For(registration.InterfaceType).Add(registration.ImplementationType).Named(registration.Name);
            else if (isSingleton)
                registry.For(registration.InterfaceType).Use(registration.ImplementationType).Singleton();
            else
                registry.For(registration.InterfaceType).Use(registration.ImplementationType);
        }
    }
}
