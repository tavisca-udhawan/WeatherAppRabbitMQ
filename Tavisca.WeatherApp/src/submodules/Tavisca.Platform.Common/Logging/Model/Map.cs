using System.Collections.Generic;

namespace Tavisca.Platform.Common.Logging
{
    public class Map
    {
        public IDictionary<string, string> Value { get; private set; }

        public Map(IDictionary<string, string> map, MapFormat format = MapFormat.DelimitedString)
        {
            Value = map;
            Format = format;
        }

        public MapFormat Format { get; set; }
    }

    public enum MapFormat
    {
        Json,
        DelimitedString
    }
}
