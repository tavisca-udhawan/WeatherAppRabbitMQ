using System;

namespace Tavisca.Platform.Common.SessionStore
{
    public class ItemKey
    {
        public ItemKey(string category, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (string.IsNullOrWhiteSpace(category))
                throw new ArgumentNullException(nameof(category));

            Key = key;
            Category = category;
        }
        public string Key { get; }
        public string Category { get; }

        public override bool Equals(object obj)
        {
            var that = obj as ItemKey;
            if (that == null)
                return false;

            if (ReferenceEquals(this, that))
                return true;

            return (this.Key == that.Key) && (this.Category == that.Category);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)2166136261;
                hash = (hash * 16777619) ^ (this.Key ?? string.Empty).GetHashCode();
                hash = (hash * 16777619) ^ (this.Category ?? string.Empty).GetHashCode();
                return hash;
            }
        }
    }
}
