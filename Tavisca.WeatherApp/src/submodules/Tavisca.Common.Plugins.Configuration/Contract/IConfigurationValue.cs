using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Configuration
{
    public interface IConfigurationValue
    {
        string GetAsString(string defaultValue = null);

        NameValueCollection GetAsNameValueCollcetion(NameValueCollection defaultValue = null);

        T GetAs<T>(T defaultValue = default(T));
    }
}
