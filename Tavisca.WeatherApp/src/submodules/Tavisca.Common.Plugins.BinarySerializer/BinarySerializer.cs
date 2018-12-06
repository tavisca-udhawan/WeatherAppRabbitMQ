using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Tavisca.Platform.Common.Serialization;

namespace Tavisca.Common.Plugins.BinarySerializer
{
    public class BinarySerializer : ISerializer
    {
        public void Serialize<T>(Stream outputStream, T data)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(outputStream, data);
        }

        public T Deserialize<T>(Stream inputStream)
        {
            var formatter = new BinaryFormatter();
            return (T)formatter.Deserialize(inputStream);
        }

        public void Serialize(Stream outputStream, object data, Type type = null)
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(outputStream, data);
        }

        public object Deserialize(Stream inputStream, Type type)
        {
            var formatter = new BinaryFormatter();
            return formatter.Deserialize(inputStream);
        }
    }
}
