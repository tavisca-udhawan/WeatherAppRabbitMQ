using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Configuration
{
    public interface IJsonSerializer
    {
        T Deserialize<T>(string value);

        string Serialize(object value);
    }
}
