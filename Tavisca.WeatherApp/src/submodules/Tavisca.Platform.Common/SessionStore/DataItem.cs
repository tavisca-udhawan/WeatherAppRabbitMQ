using System;

namespace Tavisca.Platform.Common.SessionStore
{
    public class DataItem<T>
    {
        public DataItem(ItemKey key, T value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Key = key;
            Value = value;
        }
        public ItemKey Key { get; }
        public T Value { get; }
    }
}
