using System;
using System.Collections.Generic;
using System.Linq;
using StructureMap;
using Tavisca.Platform.Common.Containers;

namespace Tavisca.Common.Plugins.StructureMap
{
    public class DependencyContainer : IDependencyContainer
    {
        public DependencyContainer(Container container)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            //removed the framework check to ensure that the Profile injector is injected for .net core as well
            container.Configure(x => x.Policies.Interceptors(new ProfilerInjectorPolicy()));
            InnerContainer = container;
        }

        public Container InnerContainer { get; }

        public object GetService(Type serviceType)
        {
            return InnerContainer.TryGetInstance(serviceType);
        }

        public object GetInstance(Type serviceType)
        {
            try
            {
                return InnerContainer.GetInstance(serviceType);
            }
            catch (StructureMapException exception)
            {
                throw new DependencyException($"No default registration found for Type {serviceType.AssemblyQualifiedName}", exception);
            }
        }

        public object GetInstance(Type serviceType, string key)
        {
            try
            {
                return InnerContainer.GetInstance(serviceType, key);
            }
            catch (StructureMapException exception)
            {
                throw new DependencyException($"No registration found for Type {serviceType.AssemblyQualifiedName} with name {key}", exception);
            }
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            try
            {
                return InnerContainer.GetAllInstances(serviceType).Cast<object>();
            }
            catch (StructureMapException exception)
            {
                throw new DependencyException($"No registration found for Type {serviceType.AssemblyQualifiedName}", exception);
            }
        }

        public TService GetInstance<TService>()
        {
            try
            {
                return InnerContainer.GetInstance<TService>();
            }
            catch (StructureMapException exception)
            {
                throw new DependencyException($"No default registration found for Type {typeof(TService).AssemblyQualifiedName}", exception);
            }
        }

        public TService GetInstance<TService>(string key)
        {
            try
            {
                return InnerContainer.GetInstance<TService>(key);
            }
            catch (StructureMapException exception)
            {
                throw new DependencyException($"No registration found for Type {typeof(TService).AssemblyQualifiedName} with name {key}", exception);
            }
        }

        public IDependencyContainer Register(Type contract, Type implementation)
        {
            InnerContainer.Configure(c => c.For(contract).Use(implementation));
            return this;
        }

        public IDependencyContainer Register(Type contract, Type implementation, string name)
        {
            InnerContainer.Configure(c => c.For(contract).Use(implementation).Named(name));
            return this;
        }

        public IDependencyContainer Register(Type contract, Func<object> constructor)
        {
            InnerContainer.Configure(c => c.For(contract).Use(constructor));
            return this;
        }

        public IDependencyContainer Register(Type contract, Func<object> constructor, string name)
        {
            InnerContainer.Configure(c => c.For(contract).Use(constructor).Named(name));
            return this;
        }

        public IDependencyContainer RegisterAsSingleton(Type contract, Type implementation)
        {
            InnerContainer.Configure(c => c.For(contract).Use(implementation).Singleton());
            return this;
        }

        public IDependencyContainer RegisterAsSingleton(Type contract, Type implementation, string name)
        {
            InnerContainer.Configure(c => c.For(contract).Use(implementation).Named(name).Singleton());
            return this;
        }

        public IDependencyContainer RegisterAsSingleton(Type contract, Func<object> constructor)
        {
            InnerContainer.Configure(c => c.For(contract).Use(constructor).Singleton());
            return this;
        }

        public IDependencyContainer RegisterAsSingleton(Type contract, Func<object> constructor, string name)
        {
            InnerContainer.Configure(c => c.For(contract).Use(constructor).Named(name).Singleton());
            return this;
        }

        public IDependencyContainer RegisterAsSingleton<TInterface, TImpl>()
            where TImpl : TInterface
        {
            InnerContainer.Configure(c => c.For<TInterface>().Use<TImpl>().Singleton());
            return this;
        }

        public IDependencyContainer RegisterAsSingleton<TInterface, TImpl>(string name)
            where TImpl : TInterface
        {
            InnerContainer.Configure(c => c.For<TInterface>().Use<TImpl>().Named(name).Singleton());
            return this;
        }

        public IDependencyContainer RegisterAsSingleton<TInterface>(Func<TInterface> constructor)
        {
            InnerContainer.Configure(c => c.For<TInterface>().Use(() => constructor()).Singleton());
            return this;
        }

        public IDependencyContainer RegisterAsSingleton<TInterface>(Func<TInterface> constructor, string name)
        {
            InnerContainer.Configure(c => c.For<TInterface>().Use(() => constructor()).Named(name).Singleton());
            return this;
        }

        public IDependencyContainer Register<TInterface, TImpl>()
            where TImpl : TInterface
        {
            InnerContainer.Configure(c => c.For<TInterface>().Use<TImpl>());
            return this;
        }

        public IDependencyContainer Register<TInterface, TImpl>(string name)
            where TImpl : TInterface
        {
            InnerContainer.Configure(c => c.For<TInterface>().Use<TImpl>().Named(name));
            return this;
        }

        public IDependencyContainer Register<TInterface>(Func<TInterface> constructor)
        {
            InnerContainer.Configure(c => c.For<TInterface>().Use(() => constructor()).Singleton());
            return this;
        }

        public IDependencyContainer Register<TInterface>(Func<TInterface> constructor, string name)
        {
            InnerContainer.Configure(c => c.For<TInterface>().Use(() => constructor()).Named(name));
            return this;
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            try
            {
                return InnerContainer.GetAllInstances<TService>();
            }
            catch (StructureMapException exception)
            {
                throw new DependencyException($"No registration found for Type {typeof(TService).AssemblyQualifiedName}", exception);
            }
        }
    }
}
