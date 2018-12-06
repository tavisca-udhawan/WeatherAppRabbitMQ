using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tavisca.Common.Plugins.Configuration
{
    public static class StringValidator
    {
        public static void ValidateIsNullOrEmpty(string input,string name)
        {
            if (string.IsNullOrEmpty(input))
                throw Errors.ClientSide.InvalidValue(name);
        }
    }
}
