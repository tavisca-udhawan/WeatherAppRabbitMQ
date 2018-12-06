using System;

namespace Tavisca.Platform.Common.Logging
{
    public interface IMask
    {
        string Mask(string value);
    }

    public class FuncMask : IMask
    {
        private Func<string, string> _mask;

        public FuncMask(Func<string, string> mask)
        {
            _mask = mask;
        }

        public string Mask(string value)
        {
            return _mask(value);
        }
    }
}
