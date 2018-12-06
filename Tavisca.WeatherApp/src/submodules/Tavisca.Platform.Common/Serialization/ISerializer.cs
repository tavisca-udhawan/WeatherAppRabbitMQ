using System;
using System.IO;

namespace Tavisca.Platform.Common.Serialization
{
    public interface ISerializer
    {
        void Serialize<T>(Stream outputStream, T data);

        T Deserialize<T>(Stream inputStream);
        void Serialize(Stream outputStream, object data, Type type = null);

        object Deserialize(Stream inputStream, Type type);
    }
}
