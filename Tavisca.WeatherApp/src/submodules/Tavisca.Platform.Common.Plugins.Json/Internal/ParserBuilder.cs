using System;

namespace Tavisca.Platform.Common.Plugins.Json
{
    public static class ParserBuilder
    {
        public static JsonParserBuilder<T> ForType<T>(Func<T> constructor, T defaultValue = default(T) )
        {
            return new JsonParserBuilder<T>(constructor, defaultValue);
        }
    }
}
