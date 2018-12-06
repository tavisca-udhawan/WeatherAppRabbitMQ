using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Containers
{
    public class Registration
    {

        public static Registration AsSingleton(Type interfaceType, Type implType)
        {
            return new Registration(interfaceType, implType, null, null, true);
        }

        public static Registration AsSingleton(Type interfaceType, Type implType, string name)
        {
            return new Registration(interfaceType, implType, null, name, true);
        }

        public static Registration AsSingleton(Type interfaceType, Func<object> constructor)
        {
            return new Registration(interfaceType, null, constructor, null, true);
        }

        public static Registration AsSingleton(Type interfaceType, Func<object> constructor, string name)
        {
            return new Registration(interfaceType, null, constructor, name, true);
        }


        public static Registration AsSingleton<TContract, TImpl>()
            where TImpl : TContract
        {
            return AsSingleton(typeof(TContract), typeof(TImpl));
        }

        public static Registration AsSingleton<TContract, TImpl>(string name)
        {
            return AsSingleton(typeof(TContract), typeof(TImpl), name);
        }

        public static Registration AsSingleton<T>(Func<object> constructor)
        {
            return AsSingleton(typeof(T), constructor);
        }

        public static Registration AsSingleton<T>(Func<object> constructor, string name)
        {
            return AsSingleton(typeof(T), constructor, name);
        }

        public static Registration AsPerCall(Type interfaceType, Type implType)
        {
            return new Registration(interfaceType, implType, null, null, false);
        }

        public static Registration AsPerCall(Type interfaceType, Type implType, string name)
        {
            return new Registration(interfaceType, implType, null, name, false);
        }

        public static Registration AsPerCall(Type interfaceType, Func<object> constructor)
        {
            return new Registration(interfaceType, null, constructor, null, false);
        }

        public static Registration AsPerCall(Type interfaceType, Func<object> constructor, string name)
        {
            return new Registration(interfaceType, null, constructor, name, false);
        }

        public static Registration AsPerCall<TContract, TImpl>()
            where TImpl : TContract
        {
            return AsPerCall(typeof(TContract), typeof(TImpl));
        }

        public static Registration AsPerCall<TContract, TImpl>(string name)
        {
            return AsPerCall(typeof(TContract), typeof(TImpl), name);
        }

        public static Registration AsPerCall<T>(Func<object> constructor)
        {
            return AsPerCall(typeof(T), constructor);
        }

        public static Registration AsPerCall<T>(Func<object> constructor, string name)
        {
            return AsPerCall(typeof(T), constructor, name);
        }

        private Registration(Type interfaceType, Type implementationType, Func<object> constructor, string name,
            bool isSingleton)
        {
            InterfaceType = interfaceType;
            ImplementationType = implementationType;
            Constructor = constructor;
            Name = name;
            IsSingleton = isSingleton;
        }

        public Type InterfaceType { get; }

        public Type ImplementationType { get; }

        public string Name { get; }

        public Func<object> Constructor { get; }

        public bool IsSingleton { get; }
    }
}
