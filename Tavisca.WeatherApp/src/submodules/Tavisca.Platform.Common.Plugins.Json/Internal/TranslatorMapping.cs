using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Platform.Common.Plugins.Json
{
    public interface ITranslatorMapping
    {
        void Configure(IDictionary<Type, JsonConverter> mapping);
    }
}
