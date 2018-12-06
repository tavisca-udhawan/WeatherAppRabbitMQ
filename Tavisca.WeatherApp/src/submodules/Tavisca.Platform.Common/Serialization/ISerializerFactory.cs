using System;

namespace Tavisca.Platform.Common.Serialization
{
    public interface ISerializerFactory
    {
        ISerializer Create(Type type);
    }
}
